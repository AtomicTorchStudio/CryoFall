namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelItemDurability : BaseViewModel
    {
        private static readonly Brush BrushGreen
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColorGreen4");

        private static readonly Brush BrushRed
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColorRed5");

        private static readonly Brush BrushYellow
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColor5");

        private IItemWithDurabilityPrivateState durabilityPrivateState;

        private IItem item;

        public Brush Brush { get; private set; }

        public uint DurabilityCurrent { get; set; }

        public uint DurabilityMax { get; private set; }

        public IItem Item
        {
            get => this.item;
            set
            {
                if (this.item == value)
                {
                    return;
                }

                if (this.item is not null)
                {
                    this.ReleaseSubscriptions();
                }

                this.item = value;
                if (this.item is null)
                {
                    return;
                }

                this.DurabilityMax = ((IProtoItemWithDurability)this.item.ProtoItem).DurabilityMax;

                this.durabilityPrivateState = this.item.GetPrivateState<IItemWithDurabilityPrivateState>();
                this.durabilityPrivateState.ClientSubscribe(_ => _.DurabilityCurrent,
                                                            _ => this.Refresh(),
                                                            this);

                this.Refresh();
            }
        }

        private static Brush GetBrush(double fraction)
        {
            if (fraction >= ItemDurabilitySystem.ThresholdFractionGreenStatus)
            {
                return BrushGreen;
            }

            if (fraction >= ItemDurabilitySystem.ThresholdFractionYellowStatus)
            {
                return BrushYellow;
            }

            return BrushRed;
        }

        private void Refresh()
        {
            this.DurabilityCurrent = this.durabilityPrivateState.DurabilityCurrent;
            this.Brush = GetBrush(this.DurabilityCurrent / (double)this.DurabilityMax);
        }
    }
}