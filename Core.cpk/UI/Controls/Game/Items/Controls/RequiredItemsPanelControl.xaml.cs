namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class RequiredItemsPanelControl : BaseUserControl
    {
        private static readonly IReadOnlyList<ProtoItemWithCount> EmptyList
            = Array.Empty<ProtoItemWithCount>();

        public static readonly DependencyProperty CountMultiplierProperty = DependencyProperty.Register(
            nameof(CountMultiplier),
            typeof(ushort),
            typeof(RequiredItemsPanelControl),
            new PropertyMetadata((ushort)1, CustomDependencyPropertyChanged));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            nameof(Items),
            typeof(IReadOnlyList<ProtoItemWithCount>),
            typeof(RequiredItemsPanelControl),
            new PropertyMetadata(EmptyList, CustomDependencyPropertyChanged));

        public static readonly DependencyProperty MinSlotsCountProperty =
            DependencyProperty.Register(
                nameof(MinSlotsCount),
                typeof(byte),
                typeof(RequiredItemsPanelControl),
                new PropertyMetadata((byte)0, CustomDependencyPropertyChanged));

        public static readonly DependencyProperty IsChecksItemAvailabilityProperty =
            DependencyProperty.Register(
                nameof(IsChecksItemAvailability),
                typeof(bool),
                typeof(RequiredItemsPanelControl),
                new PropertyMetadata(true, CustomDependencyPropertyChanged));

        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
            nameof(IconSize),
            typeof(int),
            typeof(RequiredItemsPanelControl),
            new PropertyMetadata(52));

        public static readonly DependencyProperty TextStrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(TextStrokeThickness),
                typeof(double),
                typeof(RequiredItemsPanelControl),
                new PropertyMetadata(1d));

        private readonly List<RequiredItemControl> slotsControls = new();

        private UIElementCollection wrapPanelChildren;

        public ushort CountMultiplier
        {
            get => (ushort)this.GetValue(CountMultiplierProperty);
            set => this.SetValue(CountMultiplierProperty, value);
        }

        public int IconSize
        {
            get => (int)this.GetValue(IconSizeProperty);
            set => this.SetValue(IconSizeProperty, value);
        }

        public bool IsChecksItemAvailability
        {
            get => (bool)this.GetValue(IsChecksItemAvailabilityProperty);
            set => this.SetValue(IsChecksItemAvailabilityProperty, value);
        }

        public IReadOnlyList<ProtoItemWithCount> Items
        {
            get => (IReadOnlyList<ProtoItemWithCount>)this.GetValue(ItemsProperty);
            set => this.SetValue(ItemsProperty, value);
        }

        public byte MinSlotsCount
        {
            get => (byte)this.GetValue(MinSlotsCountProperty);
            set => this.SetValue(MinSlotsCountProperty, value);
        }

        public double TextStrokeThickness
        {
            get => (double)this.GetValue(TextStrokeThicknessProperty);
            set => this.SetValue(TextStrokeThicknessProperty, value);
        }

        protected override void InitControl()
        {
            this.wrapPanelChildren = this.GetByName<WrapPanel>("WrapPanel").Children;

            if (IsDesignTime)
            {
                this.wrapPanelChildren.Clear();
                for (var i = 0; i < this.MinSlotsCount; i++)
                {
                    this.wrapPanelChildren.Add(new RequiredItemControl());
                }
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            this.Refresh();

            if (!IsDesignTime)
            {
                this.SubscribeToContainersEvents();
            }
        }

        protected override void OnUnloaded()
        {
            if (!IsDesignTime)
            {
                this.UnsubscribeFromContainersEvents();
            }

            // remove all the slot controls and return them to cache
            var controlsCache = ControlsCache<RequiredItemControl>.Instance;
            this.wrapPanelChildren.Clear();
            foreach (var slotsControl in this.slotsControls)
            {
                controlsCache.Push(slotsControl);
            }

            this.slotsControls.Clear();
        }

        private static void CustomDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((RequiredItemsPanelControl)d).Refresh();
        }

        private void ContainersItemsResetHandler()
        {
            this.Refresh();
        }

        private void ItemAddedOrRemovedOrCountChangedHandler(IItem item)
        {
            if (!this.IsChecksItemAvailability)
            {
                return;
            }

            foreach (var slot in this.slotsControls)
            {
                if (slot.ProtoItemWithCount?.ProtoItem == item.ProtoGameObject)
                {
                    slot.RefreshDisplayedCount();
                }
            }
        }

        private void Refresh()
        {
            if (!this.IsLoaded
                || IsDesignTime)
            {
                return;
            }

            var items = this.Items;
            var countMultiplier = this.CountMultiplier;
            var minSlotsCount = this.MinSlotsCount;
            var itemsCount = items.Count;
            var slotsToUpdateCount = Math.Min(this.slotsControls.Count, items.Count);
            var newSlotsCount = Math.Max(minSlotsCount, items.Count);

            var iconSize = this.IconSize;
            var iconFontSize = this.FontSize;
            var textStrokeThickness = this.TextStrokeThickness;
            var isChecksItemAvailability = this.IsChecksItemAvailability;

            var controlsCache = ControlsCache<RequiredItemControl>.Instance;

            // update existing and used slot controls
            for (var i = 0; i < slotsToUpdateCount; i++)
            {
                var control = this.slotsControls[i];
                control.Set(items[i], countMultiplier, isChecksItemAvailability);
            }

            // add new slot controls
            for (var i = this.slotsControls.Count; i < newSlotsCount; i++)
            {
                var control = controlsCache.Pop();
                control.Width = control.Height = iconSize;
                control.FontSize = iconFontSize;
                control.TextStrokeThickness = textStrokeThickness;
                this.slotsControls.Add(control);

                if (i < itemsCount)
                {
                    // setup slot with current item
                    control.Set(items[i], countMultiplier, isChecksItemAvailability);
                }

                this.wrapPanelChildren.Add(control);
            }

            // remove extra slot controls
            while (this.slotsControls.Count > newSlotsCount)
            {
                var control = this.slotsControls[newSlotsCount];
                this.slotsControls.RemoveAt(newSlotsCount);
                this.wrapPanelChildren.RemoveAt(newSlotsCount);
                controlsCache.Push(control);
            }

            // reset remaining slot controls
            for (var i = itemsCount; i < minSlotsCount; i++)
            {
                this.slotsControls[i]
                    .Set(item: null, countMultiplier: 1, isChecksItemAvailability: isChecksItemAvailability);
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