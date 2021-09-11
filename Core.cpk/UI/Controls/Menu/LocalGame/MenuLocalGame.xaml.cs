namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.LocalGame
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.LocalGame.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuLocalGame : BaseUserControl
    {
        public static readonly DependencyProperty IsSelectedProperty
            = DependencyProperty.Register("IsSelected",
                                          typeof(bool),
                                          typeof(MenuLocalGame),
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
            this.layoutRoot.DataContext = new ViewModelMenuLocalGame();
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            var viewModel = (ViewModelMenuLocalGame)this.layoutRoot.DataContext;
            this.layoutRoot.DataContext = null;
            viewModel.Dispose();
        }

        private static void IsSelectedPropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MenuLocalGame)d).Refresh();
        }

        private void Refresh()
        {
            if (!this.isLoaded)
            {
                return;
            }

            var viewModel = (ViewModelMenuLocalGame)this.layoutRoot.DataContext;
            viewModel.Refresh(this.IsSelected);
        }
    }
}