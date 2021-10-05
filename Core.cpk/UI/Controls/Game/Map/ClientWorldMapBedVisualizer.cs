namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ClientWorldMapBedVisualizer : BaseWorldMapVisualizer
    {
        private readonly PlayerCharacterPrivateState playerCharacterPrivateState;

        private readonly IStateSubscriptionOwner stateSubscriptionOwner;

        private WorldMapMarkCurrentBed currentBedMark;

        public ClientWorldMapBedVisualizer(WorldMapController worldMapController)
            : base(worldMapController)
        {
            this.stateSubscriptionOwner = new StateSubscriptionStorage();
            this.playerCharacterPrivateState = ClientCurrentCharacterHelper.PrivateState;

            this.playerCharacterPrivateState.ClientSubscribe(
                _ => _.CurrentBedObjectPosition,
                _ => this.Refresh(),
                this.stateSubscriptionOwner);

            this.Refresh();
        }

        protected override void DisposeInternal()
        {
            this.stateSubscriptionOwner.ReleaseSubscriptions();
            this.DestroyControl();
        }

        private void DestroyControl()
        {
            if (this.currentBedMark is null)
            {
                return;
            }

            this.RemoveControl(this.currentBedMark);
            this.currentBedMark = null;
        }

        private void Refresh()
        {
            var bedWorldPosition = this.playerCharacterPrivateState.CurrentBedObjectPosition;
            if (!bedWorldPosition.HasValue)
            {
                this.DestroyControl();
                return;
            }

            if (this.currentBedMark is null)
            {
                this.currentBedMark = new WorldMapMarkCurrentBed();
                Panel.SetZIndex(this.currentBedMark, 9);
                this.AddControl(this.currentBedMark);
            }

            var canvasPosition = this.WorldToCanvasPosition(
                bedWorldPosition.Value.ToVector2D());

            Canvas.SetLeft(this.currentBedMark, canvasPosition.X);
            Canvas.SetTop(this.currentBedMark, canvasPosition.Y);
        }
    }
}