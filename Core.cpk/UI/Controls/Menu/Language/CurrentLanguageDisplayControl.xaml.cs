namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Language
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.ClientLanguages;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Language.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CurrentLanguageDisplayControl : BaseUserControl
    {
        public static readonly DependencyProperty ButtonCommandProperty =
            DependencyProperty.Register(nameof(ButtonCommand),
                                        typeof(BaseCommand),
                                        typeof(CurrentLanguageDisplayControl),
                                        new PropertyMetadata(default(BaseCommand)));

        private ViewModelLanguageDefinition viewModel;

        public BaseCommand ButtonCommand
        {
            get => (BaseCommand)this.GetValue(ButtonCommandProperty);
            set => this.SetValue(ButtonCommandProperty, value);
        }

        protected override void InitControl()
        {
            this.ButtonCommand = new ActionCommand(
                () => MenuLanguageSelection.IsDisplayed = true);

            ClientLanguagesManager.CurrentLanguageDefinitionChanged
                += this.CurrentLanguageDefinitionChangedHandler;
        }

        protected override void OnLoaded()
        {
            this.Refresh();
        }

        private void CurrentLanguageDefinitionChangedHandler()
        {
            this.Refresh();
        }

        private void Refresh()
        {
            this.DataContext = null;
            this.viewModel?.Dispose();
            this.viewModel = null;

            if (!this.isLoaded)
            {
                return;
            }

            var currentLanguageDefinition = ClientLanguagesManager.CurrentLanguageDefinition;
            this.DataContext = this.viewModel = new ViewModelLanguageDefinition(currentLanguageDefinition);
        }
    }
}