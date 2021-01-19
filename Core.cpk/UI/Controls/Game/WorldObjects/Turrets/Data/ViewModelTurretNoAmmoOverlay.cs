namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Turrets.Data
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ViewModelTurretNoAmmoOverlay : BaseViewModel
    {
        private readonly ObjectTurretPublicState publicState;

        public ViewModelTurretNoAmmoOverlay(IStaticWorldObject worldObject)
        {
            this.publicState = worldObject.GetPublicState<ObjectTurretPublicState>();
            this.Update();
            ClientUpdateHelper.UpdateCallback += this.Update;
        }

        public bool IsVisible { get; set; }

        protected override void DisposeViewModel()
        {
            ClientUpdateHelper.UpdateCallback -= this.Update;
            base.DisposeViewModel();
        }

        private void Update()
        {
            if (!this.publicState.HasNoAmmo)
            {
                this.IsVisible = false;
                return;
            }

            if (ClientComponentObjectInteractionHelper.MouseOverObject
                == this.publicState?.GameObject)
            {
                // mouse over - no flickering
                this.IsVisible = true;
                return;
            }

            // flicker every half second
            var time = Client.Core.ClientRealTime % 1.0;
            this.IsVisible = time < 0.5;
        }
    }
}