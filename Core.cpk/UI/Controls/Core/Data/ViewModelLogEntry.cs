namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public readonly struct ViewModelLogEntry
    {
        // collapse server log when it's not available
        private static readonly byte CachedColumnSpan = Api.IsEditor ? (byte)1 : (byte)3;

        private static readonly BaseCommand StaticCommandCopyLogEntry = new ActionCommandWithParameter(
            arg =>
            {
                var message = (string)arg;
                Api.Client.Core.CopyToClipboard(message);
            });

        public ViewModelLogEntry(
            string textMessage,
            string textTooltip,
            Brush foregroundBrush,
            BaseCommand commandOpenFile,
            bool isServerLog)
        {
            this.TextMessage = textMessage;
            this.TextTooltip = textTooltip;
            this.ForegroundBrush = foregroundBrush;
            this.CommandOpenFile = commandOpenFile;
            this.Column = (byte)(isServerLog ? 2 : 0);
        }

        public byte Column { get; }

        public byte ColumnSpan => CachedColumnSpan;

        public BaseCommand CommandCopyLogEntry => StaticCommandCopyLogEntry;

        public BaseCommand CommandOpenFile { get; }

        public Brush ForegroundBrush { get; }

        public bool IsOpenFileMenuEntryEnabled => this.CommandOpenFile is not null;

        public string TextMessage { get; }

        public string TextTooltip { get; }
    }
}