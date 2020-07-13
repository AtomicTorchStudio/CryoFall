namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.WorldObjectClaim
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WorldObjectClaimTooltip : BaseUserControl
    {
        public static readonly DependencyProperty ExpirationTimeTextProperty =
            DependencyProperty.Register(nameof(ExpirationTimeText),
                                        typeof(string),
                                        typeof(WorldObjectClaimTooltip),
                                        new PropertyMetadata(default(string)));

        private WorldObjectClaim.PublicState publicState;

        private StateSubscriptionStorage subscriptionStorage;

        public string ExpirationTimeText
        {
            get => (string)this.GetValue(ExpirationTimeTextProperty);
            set => this.SetValue(ExpirationTimeTextProperty, value);
        }

        public void Setup(ILogicObject tag)
        {
            if (this.publicState != null)
            {
                this.subscriptionStorage.ReleaseSubscriptions();
            }

            this.subscriptionStorage = new StateSubscriptionStorage();
            this.publicState = WorldObjectClaim.GetPublicState(tag);
            if (this.publicState is null)
            {
                return;
            }

            this.publicState.ClientSubscribe(_ => _.ExpirationTime,
                                             _ => this.RefreshExpirationTimeNow(),
                                             this.subscriptionStorage);

            this.RefreshExpirationTimeNow();
        }

        protected override void OnLoaded()
        {
            this.RefreshExpirationTimeScheduled();
        }

        private void RefreshExpirationTimeNow()
        {
            if (this.publicState is null)
            {
                return;
            }

            var secondsRemains = this.publicState.ExpirationTime - Api.Client.CurrentGame.ServerFrameTimeApproximated;
            secondsRemains = Math.Max(1, secondsRemains);
            this.ExpirationTimeText = string.Format(CoreStrings.WorldObjectClaim_ExpiresIn_Format,
                                                    ClientTimeFormatHelper.FormatTimeDuration(secondsRemains));
        }

        private void RefreshExpirationTimeScheduled()
        {
            if (!this.isLoaded)
            {
                return;
            }

            this.RefreshExpirationTimeNow();
            ClientTimersSystem.AddAction(1, this.RefreshExpirationTimeScheduled);
        }
    }
}