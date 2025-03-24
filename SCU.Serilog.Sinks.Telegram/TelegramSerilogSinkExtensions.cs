using Serilog.Events;
using Serilog;
using Serilog.Configuration;
namespace SCU.Serilog.Sinks.Telegram
{
    public static class TelegramSerilogSinkExtensions
    {
        /// <summary>
        /// Remember: In Serilog, sinks are singletons by default, so Telegram Sink will be the one instance for app
        /// </summary>
        public static LoggerConfiguration TelegramSerilog(this LoggerSinkConfiguration loggerConfiguration,
            string apiKey,
            string[] chatIds,
            int batchInterval = 5,
            int batchTextLength = 250,
            string[] excludedByContains = null,
            int maxCapacity = int.MaxValue,
            IFormatProvider formatProvider = null,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose)
        {
            ArgumentNullException.ThrowIfNull(apiKey);
            ArgumentNullException.ThrowIfNull(chatIds);
            return loggerConfiguration.Sink(
                new TelegramSerilogSink(formatProvider, apiKey, chatIds,
                    TimeSpan.FromSeconds(batchInterval > 0 ? batchInterval : 5),
                    batchTextLength > 0 ? batchTextLength : 250,
                    excludedByContains,
                    maxCapacity > 0 ? maxCapacity : int.MaxValue,
                    restrictedToMinimumLevel));
        }
    }
}
