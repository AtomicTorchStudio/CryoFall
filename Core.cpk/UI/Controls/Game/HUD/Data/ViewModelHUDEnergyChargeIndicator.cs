namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterEnergySystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelHUDEnergyChargeIndicator : BaseViewModel
    {
        private readonly IClientItemsContainer containerEquipment;

        private StateSubscriptionStorage containerEquipmentSubscriptions;

        public ViewModelHUDEnergyChargeIndicator()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.containerEquipment = (IClientItemsContainer)ClientCurrentCharacterHelper.Character
                .SharedGetPlayerContainerEquipment();
            this.containerEquipment.ItemAdded += this.ContainerEquipmentItemAddedHandler;
            this.containerEquipment.ItemRemoved += this.ContainerEquipmentItemRemovedHandler;
            this.containerEquipment.ItemsReset += this.ContainerEquipmentItemsResetHandler;

            this.Reset();
        }

        public Brush Icon
            => Api.Client.UI.GetTextureBrush(
                ProtoItemFuelIconColorHelper
                    .GetIconAndColor(typeof(IProtoItemFuelElectricity))
                    .icon);

        public Visibility IndicatorVisibility { get; private set; }

        public uint TotalCapacity { get; private set; } = 100;

        public uint TotalCharge { get; private set; } = 100;

        protected override void DisposeViewModel()
        {
            this.containerEquipmentSubscriptions?.Dispose();
            this.containerEquipmentSubscriptions = null;
            base.DisposeViewModel();
        }

        private void ContainerEquipmentItemAddedHandler(IItem item)
        {
            if (item.ProtoItem is IProtoItemPowerBank)
            {
                this.Reset();
            }
        }

        private void ContainerEquipmentItemRemovedHandler(IItem item, byte slotid)
        {
            if (item.ProtoItem is IProtoItemPowerBank)
            {
                this.Reset();
            }
        }

        private void ContainerEquipmentItemsResetHandler()
        {
            this.Reset();
        }

        private void Refresh()
        {
            this.TotalCharge = (uint)CharacterEnergySystem.ClientCalculateTotalEnergyCharge();
            this.TotalCapacity = CharacterEnergySystem.ClientCalculateTotalEnergyCapacity();

            this.IndicatorVisibility = this.TotalCapacity > 0
                                           ? Visibility.Visible
                                           : Visibility.Collapsed;
        }

        private void Reset()
        {
            this.containerEquipmentSubscriptions?.Dispose();
            this.containerEquipmentSubscriptions = new StateSubscriptionStorage();

            this.containerEquipmentSubscriptions = new StateSubscriptionStorage();

            var powerBanks = this.containerEquipment.Items.Where(i => i.ProtoItem is IProtoItemPowerBank)
                                 .Select(i => i);

            foreach (var powerBank in powerBanks)
            {
                powerBank.GetPrivateState<ItemPowerBankPrivateState>()
                         .ClientSubscribe(_ => _.EnergyCharge,
                                          _ => this.Refresh(),
                                          subscriptionOwner: this.containerEquipmentSubscriptions);
            }

            this.Refresh();
        }
    }
}