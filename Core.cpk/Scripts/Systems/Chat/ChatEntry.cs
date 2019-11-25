namespace AtomicTorch.CBND.CoreMod.Systems.Chat
{
    using System;

    [Serializable]
    public readonly struct ChatEntry
    {
        public readonly string From;

        public readonly bool HasSupporterPack;

        public readonly bool IsService;

        public readonly string Message;

        public readonly DateTime UtcDate;

        public ChatEntry(string from, string message, bool isService, DateTime date, bool hasSupporterPack)
        {
            this.From = from;
            this.Message = message;
            this.IsService = isService;
            this.HasSupporterPack = hasSupporterPack;
            this.UtcDate = date.ToUniversalTime();
        }
    }
}