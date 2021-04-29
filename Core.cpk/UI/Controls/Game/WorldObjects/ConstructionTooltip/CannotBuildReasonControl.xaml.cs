namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.ConstructionTooltip
{
    using System.Windows;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CannotBuildReasonControl : BaseUserControl
    {
        public static readonly DependencyProperty IsWarningProperty
            = DependencyProperty.Register(nameof(IsWarning),
                                          typeof(bool),
                                          typeof(CannotBuildReasonControl),
                                          new PropertyMetadata(false));

        public static readonly DependencyProperty TextProperty
            = DependencyProperty.Register("Text",
                                          typeof(string),
                                          typeof(CannotBuildReasonControl),
                                          new PropertyMetadata("Cannot build reason message"));

        public bool IsWarning
        {
            get => (bool)this.GetValue(IsWarningProperty);
            set => this.SetValue(IsWarningProperty, value);
        }

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
    }
}