namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.MedicalStations.Data
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ViewModelWindowMedicalStation : BaseViewModel
    {
        public ViewModelWindowMedicalStation()
        {
            var container = (IClientItemsContainer)ClientCurrentCharacterHelper.Character
                                                                               .SharedGetPlayerContainerEquipment();

            this.ViewModelSlot1 =
                new ViewModelImplantSlotOnStation(container, containerEquipmentSlotId: (byte)EquipmentType.Implant);
            this.ViewModelSlot2 =
                new ViewModelImplantSlotOnStation(container, containerEquipmentSlotId: (byte)EquipmentType.Implant + 1);
            this.ViewModelSlot3 =
                new ViewModelImplantSlotOnStation(container, containerEquipmentSlotId: (byte)EquipmentType.Implant + 2);
        }

        public ViewModelImplantSlotOnStation ViewModelSlot1 { get; }

        public ViewModelImplantSlotOnStation ViewModelSlot2 { get; }

        public ViewModelImplantSlotOnStation ViewModelSlot3 { get; }
    }
}