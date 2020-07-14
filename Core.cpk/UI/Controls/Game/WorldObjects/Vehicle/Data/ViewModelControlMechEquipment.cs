namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Vehicle.Data
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ItemContainers.Vehicles;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelControlMechEquipment : BaseViewModel, IViewModelWithActiveState
    {
        private bool isActive;

        public ViewModelControlMechEquipment(VehicleMechPrivateState mechPrivateState)
        {
            this.MechEquipmentItemsContainer = mechPrivateState.EquipmentItemsContainer;
            this.IsActive = true;
        }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;
                this.NotifyThisPropertyChanged();

                if (this.isActive)
                {
                    var currentCharacter = Api.Client.Characters.CurrentPlayerCharacter;

                    ClientContainersExchangeManager.Register(
                        this,
                        this.MechEquipmentItemsContainer,
                        allowedTargets: new[]
                        {
                            currentCharacter.SharedGetPlayerContainerInventory(),
                            currentCharacter.SharedGetPlayerContainerHotbar()
                        });
                }
                else
                {
                    ClientContainersExchangeManager.Unregister(this);
                }
            }
        }

        public IItemsContainer MechEquipmentItemsContainer { get; }

        public string VehicleWeaponSlotCaption
            => string.Format(CoreStrings.Vehicle_Mech_ItemSlot_WeaponHardpoint_Format,
                             ((BaseItemsContainerMechEquipment)this.MechEquipmentItemsContainer.ProtoItemsContainer)
                             .WeaponHardpointName
                             .GetDescription());

        protected override void DisposeViewModel()
        {
            this.IsActive = false;
            base.DisposeViewModel();
        }
    }
}