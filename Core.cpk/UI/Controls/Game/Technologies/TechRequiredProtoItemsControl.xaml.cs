namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class TechRequiredProtoItemsControl : BaseUserControl
    {
        public static readonly DependencyProperty RequiredProtoItemsProperty =
            DependencyProperty.Register("RequiredProtoItems",
                                        typeof(IReadOnlyList<IProtoItem>),
                                        typeof(TechRequiredProtoItemsControl),
                                        new PropertyMetadata(null, PropertyChangedCallback));

        public static readonly DependencyProperty TechNodeProperty =
            DependencyProperty.Register(nameof(TechNode),
                                        typeof(TechNode),
                                        typeof(TechRequiredProtoItemsControl),
                                        new PropertyMetadata(null, PropertyChangedCallback));

        private FrameworkElement layoutRoot;

        private ViewModelTechRequiredItemsControl viewModel;

        public IReadOnlyList<IProtoItem> RequiredProtoItems
        {
            get => (IReadOnlyList<IProtoItem>)this.GetValue(RequiredProtoItemsProperty);
            set => this.SetValue(RequiredProtoItemsProperty, value);
        }

        public TechNode TechNode
        {
            get => (TechNode)this.GetValue(TechNodeProperty);
            set => this.SetValue(TechNodeProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<FrameworkElement>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.CreateViewModel();
        }

        protected override void OnUnloaded()
        {
            this.TryDestroyViewModel();
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (TechRequiredProtoItemsControl)d;
            control.CreateViewModel();
        }

        private void CreateViewModel()
        {
            this.TryDestroyViewModel();

            if (!this.isLoaded)
            {
                return;
            }

            this.layoutRoot.DataContext =
                this.viewModel = new ViewModelTechRequiredItemsControl(this.RequiredProtoItems, this.TechNode);

            var textBlock = this.GetByName<TextBlock>("TextBlockRequiredProtoItems");
            textBlock.Inlines.Clear();
            textBlock.Inlines.AddRange(this.viewModel.CreateRequiredProtoItemsInlines());
        }

        private void TryDestroyViewModel()
        {
            if (this.viewModel is null)
            {
                return;
            }

            this.layoutRoot.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}