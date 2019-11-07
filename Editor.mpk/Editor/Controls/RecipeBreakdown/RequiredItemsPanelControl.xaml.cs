namespace AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown.Data;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class RequiredItemsPanelControl : BaseUserControl
    {
        private const byte MinSlotsCount = 5;

        public static readonly DependencyProperty CountMultiplierProperty = DependencyProperty.Register(
            nameof(CountMultiplier),
            typeof(ushort),
            typeof(RequiredItemsPanelControl),
            new PropertyMetadata((ushort)1, CustomDependencyPropertyChanged));

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            nameof(Items),
            typeof(IReadOnlyList<ProtoItemWithCountFractional>),
            typeof(RequiredItemsPanelControl),
            new PropertyMetadata(defaultValue: new List<ProtoItemWithCountFractional>().AsReadOnly(),
                                 CustomDependencyPropertyChanged));

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

        private readonly List<RequiredItemControl> slotsControls = new List<RequiredItemControl>();

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

        public IReadOnlyList<ProtoItemWithCountFractional> Items
        {
            get => (IReadOnlyList<ProtoItemWithCountFractional>)this.GetValue(ItemsProperty);
            set => this.SetValue(ItemsProperty, value);
        }

        public double TextStrokeThickness
        {
            get => (double)this.GetValue(TextStrokeThicknessProperty);
            set => this.SetValue(TextStrokeThicknessProperty, value);
        }

        protected override void InitControl()
        {
            this.wrapPanelChildren = this.GetByName<WrapPanel>("WrapPanel").Children;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            this.Refresh();
        }

        protected override void OnUnloaded()
        {
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

        private void Refresh()
        {
            if (!this.IsLoaded
                || IsDesignTime)
            {
                return;
            }

            var items = this.Items;
            var itemsCount = items.Count;
            var slotsToUpdateCount = Math.Min(this.slotsControls.Count, items.Count);
            var newSlotsCount = Math.Max(MinSlotsCount, items.Count);

            // ensure that slots number is always a multiple of 5
            if (newSlotsCount % 5 != 0)
            {
                newSlotsCount = (int)(5 * Math.Ceiling(newSlotsCount / 5.0));
            }

            var iconSize = this.IconSize;
            var iconFontSize = this.FontSize;
            var textStrokeThickness = this.TextStrokeThickness;

            var controlsCache = ControlsCache<RequiredItemControl>.Instance;

            // update existing and used slot controls
            for (var i = 0; i < slotsToUpdateCount; i++)
            {
                var control = this.slotsControls[i];
                control.Set(items[i]);
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
                    control.Set(items[i]);
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
            for (var i = itemsCount; i < MinSlotsCount; i++)
            {
                this.slotsControls[i].Set(item: null);
            }
        }
    }
}