namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class SelectLocationControl : BaseUserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(
                nameof(ViewModel),
                typeof(ViewModelPositionAndSizeSettings),
                typeof(SelectLocationControl),
                new PropertyMetadata(default(ViewModelPositionAndSizeSettings), ViewModelPropertyChanged));

        private Panel layoutRoot;

        public ViewModelPositionAndSizeSettings ViewModel
        {
            get => (ViewModelPositionAndSizeSettings)this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Panel>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.Refresh();
        }

        private static void ViewModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((SelectLocationControl)d).Refresh();
        }

        private void Refresh()
        {
            if (this.isLoaded)
            {
                this.layoutRoot.DataContext = this.ViewModel;
            }
        }
    }
}