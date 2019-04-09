namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Systems.FoodSpoilageSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelItemFreshness : BaseViewModel
    {
        private static readonly Brush BrushGreen = new SolidColorBrush(Color.FromArgb(0x99, 0x20, 0xC0, 0x20));

        private static readonly Brush BrushRed = new SolidColorBrush(Color.FromArgb(0xBB, 0xE0, 0x10, 0x10));

        private static readonly Brush BrushYellow = new SolidColorBrush(Color.FromArgb(0xAA, 0xE0, 0xE0, 0x10));

        private IFoodPrivateState foodPrivateState;

        private IItem item;

        public Brush Brush { get; private set; }

        public uint FreshnessCurrent { get; set; }

        public uint FreshnessMax { get; private set; }

        public IItem Item
        {
            get => this.item;
            set
            {
                if (this.item == value)
                {
                    return;
                }

                if (this.item != null)
                {
                    this.ReleaseSubscriptions();
                }

                this.item = value;
                if (this.item == null)
                {
                    return;
                }

                this.FreshnessMax = ((IProtoItemFood)this.item.ProtoItem).FreshnessMaxValue;

                if (!this.item.ClientHasPrivateState)
                {
                    // private state is not arrived!
                    // happens when player splits the item and a new item is created (without the private state)
                    return;
                }

                this.foodPrivateState = this.item.GetPrivateState<IFoodPrivateState>();
                this.foodPrivateState.ClientSubscribe(_ => _.FreshnessCurrent,
                                                      _ => this.Refresh(),
                                                      this);

                this.Refresh();
            }
        }

        private static Brush GetBrush(FoodFreshness freshness)
        {
            switch (freshness)
            {
                case FoodFreshness.Green:
                    return BrushGreen;

                case FoodFreshness.Yellow:
                    return BrushYellow;

                case FoodFreshness.Red:
                    return BrushRed;

                default:
                    throw new ArgumentOutOfRangeException(nameof(freshness), freshness, null);
            }
        }

        private void Refresh()
        {
            this.FreshnessCurrent = this.foodPrivateState.FreshnessCurrent;
            this.Brush = GetBrush(FoodSpoilageSystem.SharedGetFreshnessEnum(this.item));
        }
    }
}