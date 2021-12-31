namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ViewModelCraftingSkinPreviewControl : BaseViewModel
    {
        private IProtoItem protoItemToApply;

        public ViewModelCraftingSkinPreviewControl(Rectangle controlSkeletonView)
        {
            this.InventorySkeleton.CurrentCharacter = ClientCurrentCharacterHelper.Character;
            this.InventorySkeleton.Control = controlSkeletonView;
        }

        public ViewModelInventorySkeleton InventorySkeleton { get; }
            = new();

        public bool IsActive
        {
            get => this.InventorySkeleton.IsActive;
            set => this.InventorySkeleton.IsActive = value;
        }

        public IProtoItem ProtoItemToApply
        {
            get => this.protoItemToApply;
            set
            {
                if (this.protoItemToApply == value)
                {
                    return;
                }

                this.protoItemToApply = value;
                this.NotifyThisPropertyChanged();

                this.InventorySkeleton.OverrideProtoItem = value is not IProtoItemVehicleWeapon 
                                                               ? value
                                                               : null;
            }
        }

        protected override void DisposeViewModel()
        {
            this.InventorySkeleton.IsActive = false;
            base.DisposeViewModel();
        }
    }
}