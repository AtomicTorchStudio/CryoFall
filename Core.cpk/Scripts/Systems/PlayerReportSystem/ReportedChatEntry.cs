namespace AtomicTorch.CBND.CoreMod.Systems.PlayerReportSystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;

    [Serializable]
    public readonly struct ReportedChatEntry
    {
        public readonly ChatEntry ChatEntry;

        public readonly bool IsReportRead;

        public readonly uint ReporterCharacterId;

        public readonly string ReporterCharacterName;

        public ReportedChatEntry(
            ChatEntry chatEntry,
            uint reporterCharacterId,
            string reporterCharacterName,
            bool isReportRead)
        {
            this.ChatEntry = chatEntry;
            this.ReporterCharacterId = reporterCharacterId;
            this.ReporterCharacterName = reporterCharacterName;
            this.IsReportRead = isReportRead;
        }
    }
}