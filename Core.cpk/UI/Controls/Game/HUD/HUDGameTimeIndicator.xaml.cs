namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD
{
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDGameTimeIndicator : BaseUserControl
    {
        public static readonly DependencyProperty CurrentTimeTextProperty
            = DependencyProperty.Register(nameof(CurrentTimeText),
                                          typeof(string),
                                          typeof(HUDGameTimeIndicator),
                                          new PropertyMetadata(default(string)));

        public static readonly DependencyProperty TextBrushProperty
            = DependencyProperty.Register("TextBrush",
                                          typeof(Brush),
                                          typeof(HUDGameTimeIndicator),
                                          new PropertyMetadata(default(Brush)));

        // To easily determine the PvP/PvE/local server on a Twitch stream or let's play video,
        // we give a bit different tint to the time indicator.
        private static readonly Brush BrushLocalServer = new SolidColorBrush(Color.FromArgb(0xAA, 0xCC, 0xCC, 0xCC));

        private static readonly Brush BrushPvE = new SolidColorBrush(Color.FromArgb(0xAA, 0xBB, 0xCC, 0xFF));

        private static readonly Brush BrushPvP = new SolidColorBrush(Color.FromArgb(0xAA, 0xFF, 0xBB, 0xBB));

        private readonly StringBuilder stringBuilder = new();

        private int lastHours = int.MaxValue;

        private int lastMinutes = int.MaxValue;

        public string CurrentTimeText
        {
            get => (string)this.GetValue(CurrentTimeTextProperty);
            set => this.SetValue(CurrentTimeTextProperty, value);
        }

        public Brush TextBrush
        {
            get => (Brush)this.GetValue(TextBrushProperty);
            set => this.SetValue(TextBrushProperty, value);
        }

        protected override void OnLoaded()
        {
            ClientUpdateHelper.UpdateCallback += this.Update;
            Api.Client.LocalServer.StatusChanged += this.RefreshTextBrush;
            Api.Client.CurrentGame.ConnectionStateChanged += this.RefreshTextBrush;
            PveSystem.ClientIsPvEChanged += this.RefreshTextBrush;
            this.RefreshTextBrush();
        }

        protected override void OnUnloaded()
        {
            ClientUpdateHelper.UpdateCallback -= this.Update;
            Api.Client.LocalServer.StatusChanged -= this.RefreshTextBrush;
            Api.Client.CurrentGame.ConnectionStateChanged -= this.RefreshTextBrush;
            PveSystem.ClientIsPvEChanged -= this.RefreshTextBrush;
        }

        private void RefreshTextBrush()
        {
            if (!Api.IsEditor
                && Api.Client.CurrentGame.ConnectionState is ConnectionState.Connected
                && Api.Client.CurrentGame.ServerInfo.ServerAddress.IsLocalServer)
            {
                this.TextBrush = BrushLocalServer;
                return;
            }

            this.TextBrush = PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false)
                                 ? BrushPvE
                                 : BrushPvP;
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private void Update()
        {
            var time = TimeOfDaySystem.CurrentTimeOfDayHours;
            var hours = (int)time;
            var minutes = (int)(60 * ((time - hours) % 1.0));

            if (this.lastHours == hours
                && this.lastMinutes == minutes)
            {
                return;
            }

            this.lastHours = hours;
            this.lastMinutes = minutes;

            this.stringBuilder
                .Append(hours.ToString("00"))
                .Append(":")
                .Append(minutes.ToString("00"));

            this.CurrentTimeText = this.stringBuilder.ToString();
            this.stringBuilder.Clear();
        }
    }
}