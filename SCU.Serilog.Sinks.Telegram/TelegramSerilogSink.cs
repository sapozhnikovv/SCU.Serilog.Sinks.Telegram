using Serilog.Core;
using Serilog.Events;
using System.Threading.Channels;
using System.Text;
using Microsoft.Extensions.ObjectPool;
using Serilog.Debugging;
namespace SCU.Serilog.Sinks.Telegram
{
    /// <summary>
    /// Remember: In Serilog, sinks are singletons by default, so Telegram Sink will be the one instance for app
    /// </summary>
    public class TelegramSerilogSink : ILogEventSink
    {
        private readonly IFormatProvider _formatProvider;
        private readonly TimeSpan _batchInterval;
        private readonly int _batchTextLength;
        private readonly TelegramBot _bot;
        private readonly LogEventLevel _minimumLevel;
        private readonly string[] _excludedByContains;
        private readonly Channel<string> _channel;
        private readonly Task _ticker;
        private readonly ObjectPool<StringBuilder> _stringBuilderPool = new DefaultObjectPoolProvider().CreateStringBuilderPool();

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
            _batchTextLength = batchTextLength > 0 ? batchTextLength : 250;
            _bot = new(apiKey, chatIds);
            _minimumLevel = restrictedToMinimumLevel;
            _excludedByContains = excludedByContains ?? [];
            _channel = Channel.CreateBounded<string>(new BoundedChannelOptions(maxCapacity > 0 ? maxCapacity : int.MaxValue) { 
                SingleReader = true, 
                FullMode = BoundedChannelFullMode.DropOldest 
            });
            _ticker = Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(_batchInterval).ConfigureAwait(false);
                    try
                    {
                        var logs = _stringBuilderPool.Get();
                        try
                        {
                            while (true)
                            {
                                var isDequeued = _channel.Reader.TryRead(out var log);
                                if (!isDequeued || log is null) break;
                                logs.AppendLine(log);
                                if (logs.Length > _batchTextLength)
                                {
                                    await _bot.SendMessageAsync(logs.ToString()).ConfigureAwait(false);
                                    logs.Clear();
                                }
                            }
                            if (logs.Length > 0) await _bot.SendMessageAsync(logs.ToString()).ConfigureAwait(false);
                        }
                        finally
                        {
                            logs.Clear();
                            if (logs.Capacity < 3 * 1024) _stringBuilderPool.Return(logs);
                        }
                    }
                    catch (Exception e)
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
                    Task.Run(() => _bot.SendMessageAsync(message).ConfigureAwait(false)) : 
                    _bot.SendMessageAsync(message))
                             .GetAwaiter().GetResult();
            else _channel.Writer.TryWrite(message);
        }
    }
}
