namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays.Data
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelHotbarItemFishingRodOverlayControl : BaseViewModel
    {
        private readonly Action baitChangedCallback;

        private IItem item;

        private IProtoItemFishingBait protoItemBait;

        public ViewModelHotbarItemFishingRodOverlayControl(Action baitChangedCallback)
        {
            this.baitChangedCallback = baitChangedCallback;
            this.SubscribeToContainersEvents();
        }

        public Brush BaitIcon => Client.UI.GetTextureBrush(this.protoItemBait?.Icon);

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

                var publicState = this.item.GetPublicState<ItemFishingRodPublicState>();
                this.ProtoItemBait = publicState.CurrentProtoBait;
                publicState.ClientSubscribe(
                    _ => _.CurrentProtoBait,
                    protoBait => this.ProtoItemBait = protoBait,
                    this);
            }
        }

        public IProtoItemFishingBait ProtoItemBait
        {
            get => this.protoItemBait;
            private set
            {
                if (this.protoItemBait == value)
                {
                    return;
                }

                this.protoItemBait = value;
                this.NotifyThisPropertyChanged();

                this.NotifyPropertyChanged(nameof(this.BaitIcon));
                this.NotifyPropertyChanged(nameof(this.TotalBaitAmount));

                this.baitChangedCallback?.Invoke();
            }
        }

        public int TotalBaitAmount => this.CalculateAvailableBaitAmount();

        protected override void DisposeViewModel()
        {
            this.UnsubscribeFromContainersEvents();
            base.DisposeViewModel();
        }

        private int CalculateAvailableBaitAmount()
        {
            if (this.protoItemBait is null)
            {
                return 0;
            }

            var currentPlayerCharacter = Client.Characters.CurrentPlayerCharacter;
            return currentPlayerCharacter.CountItemsOfType(this.protoItemBait);
        }

        private void ContainersItemsResetHandler()
        {
            this.NotifyPropertyChanged(nameof(this.TotalBaitAmount));
        }

        private void ItemAddedOrRemovedOrCountChangedHandler(IItem item)
        {
            if (item.ProtoItem is IProtoItemFishingBait)
            {
                this.NotifyPropertyChanged(nameof(this.TotalBaitAmount));
            }
        }

        private void SubscribeToContainersEvents()
        {
            ClientCurrentCharacterContainersHelper.ContainersItemsReset += this.ContainersItemsResetHandler;
            ClientCurrentCharacterContainersHelper.ItemAddedOrRemovedOrCountChanged +=
                this.ItemAddedOrRemovedOrCountChangedHandler;
        }

        private void UnsubscribeFromContainersEvents()
        {
            ClientCurrentCharacterContainersHelper.ContainersItemsReset -= this.ContainersItemsResetHandler;
            ClientCurrentCharacterContainersHelper.ItemAddedOrRemovedOrCountChanged -=
                this.ItemAddedOrRemovedOrCountChangedHandler;
        }
    }
}