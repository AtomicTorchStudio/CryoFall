namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.DemoVersion.Data
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelHUDDemoVersionInfo : BaseViewModel
    {
        private static readonly IMasterServerService Info = Api.Client.MasterServer;

        private bool isDemoVersion;

        private double timeRemaining;

        public ViewModelHUDDemoVersionInfo()
        {
            Info.DemoVersionInfoChanged += this.DemoVersionInfoChangedHandler;
            ClientUpdateHelper.UpdateCallback += this.UpdateTimerOnly;
            this.Setup();
        }

        public BaseCommand CommandBuy
            => new ActionCommand(
                () => Api.Client.SteamApi.OpenBuyGamePage());

        public string DemoTimeRemainingText { get; set; }

        public float RequiredHeight { get; set; }

        public Visibility Visibility { get; private set; }

        protected override void DisposeViewModel()
        {
            Info.DemoVersionInfoChanged -= this.DemoVersionInfoChangedHandler;
            ClientUpdateHelper.UpdateCallback -= this.UpdateTimerOnly;
        }

        private void DemoVersionInfoChangedHandler()
        {
            this.Setup();
        }

        private void Setup()
        {
            this.isDemoVersion = Info.IsDemoVersion;
            this.timeRemaining = this.isDemoVersion
                                     ? Math.Max(0, Info.DemoTimeLimit - (long)Info.DemoTimePlayed)
                                     : 0;

            this.UpdateText();
        }

        private void UpdateText()
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (!this.isDemoVersion)
            {
                this.Visibility = Visibility.Collapsed;
                return;
            }

            this.Visibility = Visibility.Visible;
            this.DemoTimeRemainingText = ClientTimeFormatHelper.FormatTimeDuration(this.timeRemaining);

            ClientTimersSystem.AddAction(0.333, this.UpdateText);
        }

        private void UpdateTimerOnly()
        {
            if (!this.isDemoVersion
                || Client.CurrentGame.ConnectionState != ConnectionState.Connected)
            {
                return;
            }

            this.timeRemaining -= Client.Core.DeltaTime;
            if (this.timeRemaining <= 0)
            {
                this.timeRemaining = 0;
            }
        }
    }
}