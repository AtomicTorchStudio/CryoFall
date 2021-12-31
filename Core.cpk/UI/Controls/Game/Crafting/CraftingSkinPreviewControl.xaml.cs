namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CraftingSkinPreviewControl : BaseUserControl
    {
        public static readonly DependencyProperty ProtoItemToApplyProperty
            = DependencyProperty.Register(nameof(ProtoItemToApply),
                                          typeof(IProtoItem),
                                          typeof(CraftingSkinPreviewControl),
                                          new PropertyMetadata(default(IProtoItem), ProtoItemToApplyPropertyChanged));

        public static readonly DependencyProperty IsActiveProperty
            = DependencyProperty.Register(nameof(IsActive),
                                          typeof(bool),
                                          typeof(CraftingSkinPreviewControl),
                                          new PropertyMetadata(false, IsActivePropertyChanged));

        private Grid layoutRoot;

        private ViewModelCraftingSkinPreviewControl viewModel;

        public static bool IsDisplayed { get; private set; }

        public bool IsActive
        {
            get => (bool)this.GetValue(IsActiveProperty);
            set => this.SetValue(IsActiveProperty, value);
        }

        public IProtoItem ProtoItemToApply
        {
            get => (IProtoItem)this.GetValue(ProtoItemToApplyProperty);
            set => this.SetValue(ProtoItemToApplyProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            IsDisplayed = true;
            var controlSkeletonView = this.GetByName<Rectangle>("SkeletonViewControl");
            this.viewModel = new ViewModelCraftingSkinPreviewControl(controlSkeletonView);
            this.RefreshProtoItemToApply();
            this.RefreshIsActive();
            this.layoutRoot.DataContext = this.viewModel;
        }

        protected override void OnUnloaded()
        {
            IsDisplayed = false;
            this.layoutRoot.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private static void IsActivePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CraftingSkinPreviewControl)d).RefreshIsActive();
        }

        private static void ProtoItemToApplyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CraftingSkinPreviewControl)d).RefreshProtoItemToApply();
        }

        private void RefreshIsActive()
        {
            if (this.isLoaded)
            {
                this.viewModel.IsActive = this.IsActive;
            }
        }

        private void RefreshProtoItemToApply()
        {
            if (this.isLoaded)
            {
                this.viewModel.ProtoItemToApply = this.ProtoItemToApply;
            }
        }
    }
}