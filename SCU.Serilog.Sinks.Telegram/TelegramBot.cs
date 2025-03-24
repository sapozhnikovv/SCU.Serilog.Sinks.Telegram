namespace SCU.Serilog.Sinks.Telegram
{
    public class TelegramBot
    {
        private readonly TelegramSender _sender;
        private readonly string[] _chatIds;
        public TelegramBot(string apiKey, string[] chatIds)
        {
            _sender = new(apiKey);
            _chatIds = chatIds;
        }

        public async Task SendMessageAsync(string message)
        {
            foreach (var chatId in _chatIds) await _sender.SendMessageAsync(message, chatId).ConfigureAwait(false);
        }
    }
}
