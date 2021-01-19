namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Systems.ItemFreshnessSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelItemFreshness : BaseViewModel
    {
        private static readonly Brush BrushGreen
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColorGreen4");

        private static readonly Brush BrushRed
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColorRed5");

        private static readonly Brush BrushYellow
            = Api.Client.UI.GetApplicationResource<Brush>("BrushColor5");

        private IItem item;

        private IItemWithFreshnessPrivateState itemPrivateState;

        public Brush Brush { get; private set; }

        public uint FreshnessCurrent { get; set; }

        public uint FreshnessMax { get; private set; }

        public bool IsRefrigerated
            => ItemFreshnessSystem.SharedIsRefrigerated(this.item);

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

                this.FreshnessMax = ((IProtoItemWithFreshness)this.item.ProtoItem).FreshnessMaxValue;

                if (!this.item.ClientHasPrivateState)
                {
                    // private state is not arrived!
                    // happens when player splits the item and a new item is created (without the private state)
                    return;
                }

                this.itemPrivateState = this.item.GetPrivateState<IItemWithFreshnessPrivateState>();
                this.itemPrivateState.ClientSubscribe(_ => _.FreshnessCurrent,
                                                      _ => this.RefreshFreshnessCurrent(),
                                                      this);

                this.RefreshFreshnessCurrent();

                this.RefreshTimeToSpoil();
            }
        }

        public string SpoilsInText
        {
            get
            {
                var timeRemainingSeconds = ItemFreshnessSystem.SharedCalculateTimeToSpoilRemains(this.item);
                var textDuration = double.IsNaN(timeRemainingSeconds)
                                       ? CoreStrings.Item_SpoiledIn_Never
                                       : ClientTimeFormatHelper.FormatTimeDuration(timeRemainingSeconds,
                                           appendSeconds: false);

                return string.Format(CoreStrings.Item_SpoiledIn_Format,
                                     textDuration);
            }
        }

        private static Brush GetBrush(ItemFreshness freshness)
        {
            return freshness switch
            {
                ItemFreshness.Green  => BrushGreen,
                ItemFreshness.Yellow => BrushYellow,
                ItemFreshness.Red    => BrushRed,
                _                    => throw new ArgumentOutOfRangeException(nameof(freshness), freshness, null)
            };
        }

        private void RefreshFreshnessCurrent()
        {
            this.FreshnessCurrent = this.itemPrivateState.FreshnessCurrent;
            this.Brush = GetBrush(ItemFreshnessSystem.SharedGetFreshnessEnum(this.item));
        }

        private void RefreshTimeToSpoil()
        {
            if (this.IsDisposed)
            {
                return;
            }

            this.NotifyPropertyChanged(nameof(this.SpoilsInText));
            this.NotifyPropertyChanged(nameof(this.IsRefrigerated));

            ClientTimersSystem.AddAction(delaySeconds: 1,
                                         this.RefreshTimeToSpoil);
        }
    }
}