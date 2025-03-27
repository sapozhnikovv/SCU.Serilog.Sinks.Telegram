using SCU.MemoryChunks;
using Serilog.Debugging;
using System.Net.Http.Json;
namespace SCU.Serilog.Sinks.Telegram
{
    public class TelegramSender(string apiKey)
    {
        /// <summary>
        /// Sender settings can be changed on the fly to suit your needs. The default values ​​are suitable for most projects (small business).
        /// </summary>
        public class Settings
        {
            private static int chunkSize = 3000;
            private static int retryWaitTime = 40;

            /// <summary>
            /// It it needed to avoid TooManyRequestsSeconds. It is wait time between The chunk sendings. Values <= 0 mean No wait time.
            /// </summary>
            public static int DefaultWaitTimeAfterSendMs = 750;
            /// <summary>
            /// The max number of characters in one message can be sent. Must be > 0
            /// </summary>
            public static int ChunkSize { get => chunkSize; set => chunkSize = value > 0 ? value : throw new ArgumentException(nameof(value)); }
            /// <summary>
            /// result wait time = this value will be multiplied by ErrorsCount/TryCount. Must be > 0
            /// </summary>
            public static int RetryWaitTimeWhenTooManyRequestsSeconds { get => retryWaitTime; set => retryWaitTime = value > 0 ? value : throw new ArgumentException(nameof(value)); }

            /// <summary>
            /// Values <= 0 mean No retries
            /// </summary>
            public static int RetryCountWhenTooManyRequests = 2;
        }
        private static readonly HttpClient _httpClient = new();
        private readonly string _baseUrl = $"https://api.telegram.org/bot{apiKey}/sendMessage";
        public async Task SendMessageAsync(string message, string chatId)
        {
            try
            {
                var chunks = message.Length > Settings.ChunkSize ? 
                                 message.MemoryChunks(Settings.ChunkSize).Select(_ => _.ToString()) :
                                [message];
                foreach (var mess in chunks) 
                {
                    var errorCount = 0;
                    do
                    {
                        using var response = await _httpClient.PostAsJsonAsync(_baseUrl, new
                        {
                            chat_id = chatId,
                            text = mess
                        }).ConfigureAwait(false);
                        if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                        {
                            errorCount++;
                            var time = Settings.RetryWaitTimeWhenTooManyRequestsSeconds * errorCount;
                            SelfLog.WriteLine("TelegaSender TooManyRequests: will wait {0}s", time);
                            await Task.Delay(TimeSpan.FromSeconds(time)).ConfigureAwait(false);
                        }
                        else
                        {
                            if (errorCount != 0) errorCount = 0;
                            if (!response.IsSuccessStatusCode) SelfLog.WriteLine("TelegaSender SendMessage IsNotSuccess. Code {0}", response.StatusCode);
                        }
                    } while (errorCount > 0 && errorCount <= Settings.RetryCountWhenTooManyRequests);
                    if (Settings.DefaultWaitTimeAfterSendMs > 0) await Task.Delay(Settings.DefaultWaitTimeAfterSendMs).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                SelfLog.WriteLine("TelegaSender SendMessage error {0}", e);
            }
        }
    }
}
