using Microsoft.Extensions.ObjectPool;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;

namespace SCU.Serilog.Sinks.Telegram
{
    /// <summary>
    /// Remember: In Serilog, sinks are singletons by default, so Telegram Sink will be the one instance for app
    /// </summary>
    public sealed class TelegramSerilogSink : ILogEventSink, IDisposable, IAsyncDisposable
    {
        public sealed class Settings
        {
            /// <summary>
            /// Default value is 2 seconds
            /// </summary>
            public static TimeSpan DisposeTimeout = TimeSpan.FromSeconds(2);
        }

        private readonly IFormatProvider _formatProvider;
        private readonly TimeSpan _batchInterval;
        private readonly int _batchTextLength;
        private readonly TelegramBot _bot;
        private readonly LogEventLevel _minimumLevel;
        private readonly string[] _excludedByContains;
        private readonly Channel<string> _channel;
        private readonly Task _ticker;
        private readonly ObjectPool<StringBuilder> _stringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool();
        private readonly CancellationTokenSource _cancellationTokenSource = new();

        public TelegramSerilogSink(
            IFormatProvider formatProvider, 
            string apiKey, string[] chatIds, 
            TimeSpan batchInterval, int batchTextLength, 
            string[] excludedByContains, 
            int maxCapacity, 
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose)
        {
            _formatProvider = formatProvider;
            _batchInterval = batchInterval.TotalSeconds > 0 ? batchInterval : TimeSpan.FromSeconds(5);
            _batchTextLength = batchTextLength > 0 ? batchTextLength : 1500;
            _bot = new(apiKey, chatIds);
            _minimumLevel = restrictedToMinimumLevel;
            _excludedByContains = excludedByContains ?? [];
            _channel = Channel.CreateBounded<string>(new BoundedChannelOptions(maxCapacity > 0 ? maxCapacity : int.MaxValue) { 
                SingleReader = true, 
                FullMode = BoundedChannelFullMode.DropOldest 
            });
            _ticker = Task.Run(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(_batchInterval, _cancellationTokenSource.Token);
                        var logs = _stringBuilderPool.Get();
                        try
                        {
                            while (!_cancellationTokenSource.IsCancellationRequested)
                            {
                                var isDequeued = _channel.Reader.TryRead(out var log);
                                if (!isDequeued || log is null) break;
                                logs.AppendLine(log);
                                if (logs.Length > _batchTextLength)
                                {
                                    await _bot.SendMessageAsync(logs.ToString(), _cancellationTokenSource.Token);
                                    logs.Clear();
                                }
                            }
                            if (logs.Length > 0) await _bot.SendMessageAsync(logs.ToString(), _cancellationTokenSource.Token);
                        }
                        finally
                        {
                            logs.Clear();
                            if (logs.Capacity < 3 * 1024) _stringBuilderPool.Return(logs);
                        }
                    }
                    catch (Exception e) when (e is not OperationCanceledException)
                    {
                        SelfLog.WriteLine("TelegramSerilogSink main loop error {0}", e);
                    }
                }
            });
        }

        public static readonly string NodeID = Guid.NewGuid().ToString()[..5];
        public void Emit(LogEvent logEvent)
        {
            if (logEvent.Level < _minimumLevel) return;
            var message = logEvent.RenderMessage(_formatProvider);
            message = $"{logEvent.Timestamp:dd.MM.yy HH:mm:ss.fff zz} [{logEvent.Level}] [AppInstanceID:{NodeID}] {message}{(logEvent.Exception!=null?"\n":"")}{logEvent.Exception}";
            foreach (var excl in _excludedByContains) if (message.ToLower().Contains(excl.ToLower())) return;
            if (logEvent.Level == LogEventLevel.Fatal) 
                (SynchronizationContext.Current != null ? 
                    Task.Run(() => _bot.SendMessageAsync(message, _cancellationTokenSource.Token)) : 
                    _bot.SendMessageAsync(message, _cancellationTokenSource.Token))
                             .GetAwaiter().GetResult();
            else _channel.Writer.TryWrite(message);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SafeExecute(Action action, string logMessage)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception e)
            {
                SelfLog.WriteLine($"TelegramSerilogSink {logMessage} {{0}}", e);
            }
        }

        private int isDisposed;
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref isDisposed, 1) != 0) return;
            SafeExecute(() => _channel.Writer.Complete(), "Dispose - Closing Channel queue");
            SafeExecute(_cancellationTokenSource.Cancel, "Dispose - tokenSource.Cancel()");
            try
            {
                var timeoutTask = Task.Delay(Settings.DisposeTimeout);
                var completedTask = await Task.WhenAny(_ticker, timeoutTask).ConfigureAwait(false);
                if (completedTask == timeoutTask)
                    SelfLog.WriteLine("TelegramSerilogSink Dispose - Waiting for finishing sender task - " +
                                      "waiting failed with TelegramSerilogSink.Settings.DisposeTimeout {0}", Settings.DisposeTimeout);
            }
            catch (Exception e)
            {
                SelfLog.WriteLine("TelegramSerilogSink Dispose - Waiting for finishing sender task - error {0}", e);
            }
            SafeExecute(_bot.Dispose, "Dispose - Waiting for closing sender");
            SafeExecute(_cancellationTokenSource.Dispose, "Dispose - tokenSource.Dispose()");
        }
        public void Dispose() => Task.Run(DisposeAsync).GetAwaiter().GetResult();
    }
}
