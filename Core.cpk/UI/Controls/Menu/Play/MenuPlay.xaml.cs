namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Play
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Play.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuPlay : BaseUserControl
    {
        public static readonly DependencyProperty IsSelectedProperty
            = DependencyProperty.Register("IsSelected",
                                          typeof(bool),
                                          typeof(MenuPlay),
                                          new PropertyMetadata(default(bool), IsSelectedPropertyChangedHandler));

        private Grid layoutRoot;

        public bool IsSelected
        {
            get => (bool)this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
        }

        protected override void OnLoaded()
        {
            this.layoutRoot.DataContext = new ViewModelMenuPlay();
        }

        protected override void OnUnloaded()
        {
            var viewModel = (ViewModelMenuPlay)this.layoutRoot.DataContext;
            this.layoutRoot.DataContext = null;
            viewModel.Dispose();
        }

        private static void IsSelectedPropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var isSelected = (bool)e.NewValue;
            if (isSelected)
            {
                return;
            }

            // let's reset the selected tab
            var menuPlay = (MenuPlay)d;
            var viewModel = menuPlay?.layoutRoot?.DataContext as ViewModelMenuPlay;
            if (viewModel is null)
            {
                return;
            }

            viewModel.IsTabMultiplayerSelected = false;
            viewModel.IsTabLocalServerSelected = false;
            viewModel.IsAnyTabSelected = false;
        }
    }
}