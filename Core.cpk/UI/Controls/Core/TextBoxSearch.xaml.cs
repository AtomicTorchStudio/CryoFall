namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using System.Windows.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class TextBoxSearch : BaseUserControl
    {
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText),
                                        typeof(string),
                                        typeof(TextBoxSearch),
                                        new FrameworkPropertyMetadata(defaultValue: string.Empty)
                                        {
                                            BindsTwoWayByDefault = true,
                                            DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                                        });

        public BaseCommand CommandSearchReset => new ActionCommand(this.ExecuteCommandSearchReset);

        public string SearchText
        {
            get => (string)this.GetValue(SearchTextProperty);
            set => this.SetValue(SearchTextProperty, value);
        }

        protected override void InitControl()
        {
        }

        private void ExecuteCommandSearchReset()
        {
            this.SearchText = string.Empty;
        }
    }
}