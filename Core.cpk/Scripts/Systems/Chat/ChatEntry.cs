namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using System;

    [Serializable]
    public readonly struct ChatEntry
    {
        public readonly string From;

        public readonly bool IsService;

        public readonly string Message;

        public readonly DateTime UtcDate;

        public ChatEntry(string from, string message, bool isService, DateTime date)
        {
            this.From = from;
            this.Message = message;
            this.IsService = isService;
            this.UtcDate = date.ToUniversalTime();
        }
    }
}