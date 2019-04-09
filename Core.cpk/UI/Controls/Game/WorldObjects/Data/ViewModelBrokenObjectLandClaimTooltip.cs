namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Timer;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ViewModelBrokenObjectLandClaimTooltip : BaseViewModel
    {
        public const double TimerRefreshIntervalSeconds = 1 / 60.0;

        private readonly IStaticWorldObject objectLandClaim;

        private readonly IProtoObjectLandClaim protoObjectLandClaim;

        private readonly ObjectLandClaimPublicState publicState;

        private double? destroyTime;

        public ViewModelBrokenObjectLandClaimTooltip(
            IStaticWorldObject objectLandClaim,
            ObjectLandClaimPublicState publicState)
        {
            this.objectLandClaim = objectLandClaim;
            this.protoObjectLandClaim = (IProtoObjectLandClaim)objectLandClaim.ProtoStaticWorldObject;
            this.publicState = publicState;

            publicState.ClientSubscribe(
                _ => _.ServerTimeForDestruction,
                isWatered => { this.RefreshDataFromServer(); },
                this);

            this.RefreshDataFromServer();
        }

        public ViewModelBrokenObjectLandClaimTooltip()
        {
        }

        public float DestroyTimeoutEndsTimePercent { get; private set; } = 50;

        public string DestroyTimeText { get; private set; } = "23h 59m 59s";

        public Visibility Visibility { get; set; } = IsDesignTime ? Visibility.Visible : Visibility.Hidden;

        private static double GetTimeRemainingSeconds(double serverTime)
        {
            if (serverTime == double.MaxValue)
            {
                return serverTime;
            }

            var delta = serverTime - Client.CurrentGame.ServerFrameTimeApproximated;
            if (delta < 0)
            {
                delta = 0;
            }

            return delta;
        }

        private void RefreshDataFromServer()
        {
            if (!this.publicState.ServerTimeForDestruction.HasValue)
            {
                this.Visibility = Visibility.Collapsed;
                this.destroyTime = null;
                return;
            }

            this.Visibility = Visibility.Visible;
            this.destroyTime = this.publicState.ServerTimeForDestruction.Value;

            // start updating displayed time
            this.TimerUpdateDisplayedTime();
        }

        private void TimerUpdateDisplayedTime()
        {
            if (this.IsDisposed)
            {
                // view model is disposed - stop updating
                return;
            }

            this.UpdateDisplayedTimeNoTimer();

            // schedule refresh of the displayed time
            ClientComponentTimersManager.AddAction(
                TimerRefreshIntervalSeconds,
                this.TimerUpdateDisplayedTime);
        }

        private void UpdateDisplayedTimeNoTimer()
        {
            if (!this.destroyTime.HasValue)
            {
                this.DestroyTimeoutEndsTimePercent = 0;
                return;
            }

            var totalDuration = this.protoObjectLandClaim.DestructionTimeout.TotalSeconds;
            var timeRemainingSeconds = GetTimeRemainingSeconds(this.destroyTime.Value);
            timeRemainingSeconds = MathHelper.Clamp(timeRemainingSeconds, 0, totalDuration);
            this.DestroyTimeText = ClientTimeFormatHelper.FormatTimeDuration(timeRemainingSeconds);
            this.DestroyTimeoutEndsTimePercent = (float)(100 * timeRemainingSeconds / totalDuration);
        }
    }
}