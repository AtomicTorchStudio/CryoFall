namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;

    /// <summary>
    /// Represents view model for game item (stored in items containers).
    /// </summary>
    public class ViewModelItem : BaseViewModel
    {
        private readonly IClientItem item;

        public ViewModelItem(IItem item)
        {
            this.item = (IClientItem)item
                        ?? throw new Exception("Item should not be null. Use ViewModelItem.Empty in that case.");

            this.Count = item.Count;

            if (IsDesignTime)
            {
                return;
            }

            this.SubscribeToEvents();

            if (!(item.ProtoItem is IProtoItemWithSlotOverlay protoItemWithSlotOverlay))
            {
                return;
            }

            var controls = new List<Control>();
            protoItemWithSlotOverlay.ClientCreateItemSlotOverlayControls(item, controls);
            this.OverlayControls = controls;
        }

        /// <summary>
        /// Should be used only for creation ViewModelItem.Empty instance.
        /// </summary>
        public ViewModelItem()
        {
        }

        public ushort Count { get; private set; }

        public Visibility CountVisibility
        {
            get
            {
                if (this.item is null)
                {
                    return Visibility.Collapsed;
                }

                return this.item.ProtoItem.IsStackable ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Brush Icon =>
            !IsDesignTime
                ? (Brush)Client.UI.GetTextureBrush(this.item?.ProtoItem.Icon)
                : Brushes.BlueViolet;

        public IClientItem Item => this.item;

        public IReadOnlyList<Control> OverlayControls { get; }

        public string Title => this.item?.ProtoItem.Name ?? "Item title";

        public Visibility Visibility => this.item is null && !IsDesignTime
                                            ? Visibility.Collapsed
                                            : Visibility.Visible;

        protected override void DisposeViewModel()
        {
            this.UnsubscribeFromEvents();
        }

        private void ItemCountChangedHandler(IItem item, ushort oldCount, ushort newCount)
        {
            this.Count = newCount;
        }

        private void SubscribeToEvents()
        {
            this.item.CountChanged += this.ItemCountChangedHandler;
        }

        private void UnsubscribeFromEvents()
        {
            if (this.item is not null)
            {
                this.item.CountChanged -= this.ItemCountChangedHandler;
            }
        }
    }
}