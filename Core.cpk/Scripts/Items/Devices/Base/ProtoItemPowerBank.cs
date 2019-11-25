namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterEnergySystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public abstract class ProtoItemPowerBank
        : ProtoItemEquipmentDevice
          <ItemPowerBankPrivateState,
              EmptyPublicState,
              EmptyClientState>, IProtoItemPowerBank
    {
        public abstract uint EnergyCapacity { get; }

        public override bool OnlySingleDeviceOfThisProtoAppliesEffect => false;

        public override void ClientCreateItemSlotOverlayControls(IItem item, List<Control> controls)
        {
            controls.Add(ItemSlotEnergyChargeOverlayControl.Create(item));
            base.ClientCreateItemSlotOverlayControls(item, controls);
        }

        public override void ClientTooltipCreateControls(IItem item, List<Control> controls)
        {
            controls.Add(ItemTooltipInfoEnergyChargeControl.Create(item));
            base.ClientTooltipCreateControls(item, controls);
        }

        public override void ServerOnDestroy(IItem gameObject)
        {
            // try to redistribute remaining energy to other energy bank devices
            var energyRemains = GetPrivateState(gameObject).EnergyCharge;
            CharacterEnergySystem.ServerAddEnergyCharge(gameObject.Container,
                                                        energyRemains);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            var privateState = data.PrivateState;

            if (!data.IsFirstTimeInit)
            {
                // clamp energy charge to not exceed the capacity
                privateState.EnergyCharge = Math.Min(this.EnergyCapacity, privateState.EnergyCharge);
            }
        }
    }
}