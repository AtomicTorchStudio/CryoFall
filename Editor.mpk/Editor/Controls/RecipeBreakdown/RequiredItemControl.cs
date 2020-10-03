namespace AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class RequiredItemControl : BaseControl, ICacheableControl
    {
        public static readonly DependencyProperty TextStrokeThicknessProperty =
            DependencyProperty.Register(
                nameof(TextStrokeThickness),
                typeof(double),
                typeof(RequiredItemControl),
                new PropertyMetadata(1.5d));

        private readonly ViewModelRequiredItemControl viewModel = new ViewModelRequiredItemControl(null);

        private FrameworkElement layoutRoot;

        private ProtoItemWithCountFractional protoItemWithCount;

        private FrameworkElement tooltip;

        static RequiredItemControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(RequiredItemControl),
                new FrameworkPropertyMetadata(typeof(RequiredItemControl)));
        }

        public RequiredItemControl()
        {
        }

        public ProtoItemWithCountFractional ProtoItemWithCount => this.protoItemWithCount;

        public double TextStrokeThickness
        {
            get => (double)this.GetValue(TextStrokeThicknessProperty);
            set => this.SetValue(TextStrokeThicknessProperty, value);
        }

        public void ResetControlForCache()
        {
            this.viewModel.ProtoItemWithCount = this.protoItemWithCount = null;
        }

        public void Set(ProtoItemWithCountFractional item)
        {
            this.protoItemWithCount = item;
            this.viewModel.ProtoItemWithCount = item;
        }

        protected override void InitControl()
        {
            this.DataContext = this.viewModel;

            // this is a cached control so it's fine to keep the subscriptions forever
            this.MouseEnter += this.SlotMouseEnterHandler;
            this.MouseLeave += this.SlotMouseLeaveHandler;
        }

        protected override void OnLoaded()
        {
            // this cannot be done in InitControl as this a cached control which changes the visual tree often
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            this.layoutRoot = templateRoot.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnUnloaded()
        {
            this.DestroyTooltip();
            this.layoutRoot = null;
        }

        private void DestroyTooltip()
        {
            if (this.tooltip is null)
            {
                return;
            }

            ToolTipServiceExtend.SetToolTip(this.layoutRoot, null);
            this.tooltip = null;
        }

        private void RefreshTooltip()
        {
            this.DestroyTooltip();

            if (!this.IsMouseOver
                || this.ProtoItemWithCount is null)
            {
                // no need to display the tooltip
                return;
            }

            this.tooltip = ItemTooltipControl.Create(this.ProtoItemWithCount.ProtoItem);
            ToolTipServiceExtend.SetToolTip(this.layoutRoot, this.tooltip);
        }

        private void SlotMouseEnterHandler(object sender, MouseEventArgs e)
        {
            this.RefreshTooltip();
        }

        private void SlotMouseLeaveHandler(object sender, MouseEventArgs e)
        {
            this.DestroyTooltip();
        }
    }
}