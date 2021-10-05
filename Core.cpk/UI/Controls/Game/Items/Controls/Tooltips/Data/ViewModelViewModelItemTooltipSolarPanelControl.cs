namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data
{
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelItemTooltipSolarPanelControl : BaseViewModel
    {
        private readonly ItemWithDurabilityPrivateState itemPrivateState;

        private readonly IProtoItemSolarPanel protoItemSolarPanel;

        public ViewModelItemTooltipSolarPanelControl(IProtoItemSolarPanel protoItemSolarPanel)
        {
            this.protoItemSolarPanel = protoItemSolarPanel;
            this.Refresh();
        }

        public ViewModelItemTooltipSolarPanelControl(IItem item)
        {
            this.protoItemSolarPanel = (IProtoItemSolarPanel)item.ProtoItem;
            this.Refresh();

            this.itemPrivateState = item.GetPrivateState<ItemWithDurabilityPrivateState>();
            this.itemPrivateState.ClientSubscribe(_ => _.DurabilityCurrent,
                                                  _ => this.Refresh(),
                                                  this);
            this.Refresh();
        }

        public double EnergyAmount { get; private set; }

        public double EnergyTotal { get; private set; }

        public string LabelFormat => "{0:N0} " + CoreStrings.EnergyUnitAbbreviation;

        private void Refresh()
        {
            var durabilityDecreasePerSecond = this.protoItemSolarPanel.DurabilityDecreasePerMinuteWhenInstalled / 60.0;
            var totalLifetime = this.protoItemSolarPanel.DurabilityMax
                                / durabilityDecreasePerSecond;

            this.EnergyTotal = totalLifetime * this.protoItemSolarPanel.ElectricityProductionPerSecond;

            if (this.itemPrivateState is null)
            {
                this.EnergyAmount = this.EnergyTotal;
                return;
            }

            var remainingLifetime = this.itemPrivateState.DurabilityCurrent
                                    / durabilityDecreasePerSecond;
            this.EnergyAmount = remainingLifetime * this.protoItemSolarPanel.ElectricityProductionPerSecond;
        }
    }
}