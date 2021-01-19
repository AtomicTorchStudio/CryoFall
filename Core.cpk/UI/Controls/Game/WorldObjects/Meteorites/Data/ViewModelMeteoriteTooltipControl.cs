namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Meteorites.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelMeteoriteTooltipControl : BaseViewModel
    {
        private readonly double cooldownUntil;

        public ViewModelMeteoriteTooltipControl(IStaticWorldObject worldObjectMeteorite)
        {
            this.cooldownUntil = worldObjectMeteorite.GetPublicState<ObjectMineralMeteoritePublicState>()
                                                     .CooldownUntilServerTime;

            this.RefreshTimeRemains();
        }

        public string CooldownTimeRemainsText
            => ClientTimeFormatHelper.FormatTimeDuration(
                this.CooldownTimeRemains);

        public bool IsTooHotForMining => this.CooldownTimeRemains > 0;

        private double CooldownTimeRemains
        {
            get
            {
                var timeRemains = this.cooldownUntil - Api.Client.CurrentGame.ServerFrameTimeApproximated;
                return Math.Max(0, timeRemains);
            }
        }

        private void RefreshTimeRemains()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.NotifyPropertyChanged(nameof(this.CooldownTimeRemainsText));
            this.NotifyPropertyChanged(nameof(this.IsTooHotForMining));

            // schedule recursive update in a second
            ClientTimersSystem.AddAction(
                delaySeconds: 1,
                this.RefreshTimeRemains);
        }
    }
}