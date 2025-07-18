namespace SCU.Serilog.Sinks.Telegram
{
    public sealed class TelegramBot: IDisposable
    {
        private readonly TelegramSender _sender;
        private readonly string[] _chatIds;
        public TelegramBot(string apiKey, string[] chatIds)
        {
            _sender = new(apiKey);
            _chatIds = chatIds;
        }

        public async Task SendMessageAsync(string message, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;
            foreach (var chatId in _chatIds) await _sender.SendMessageAsync(message, chatId, token).ConfigureAwait(false);
        }

        private int isDisposed;
        public void Dispose()
        {
            if (Interlocked.Exchange(ref isDisposed, 1) != 0) return;
            _sender.Dispose();
        }
    }
}
