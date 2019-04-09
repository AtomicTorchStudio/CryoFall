namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System.Windows;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WindowContainerHelpEntry : BaseUserControl
    {
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(nameof(Description),
                                        typeof(string),
                                        typeof(WindowContainerHelpEntry),
                                        new PropertyMetadata(default(string)));

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title),
                                        typeof(string),
                                        typeof(WindowContainerHelpEntry),
                                        new PropertyMetadata(default(string)));

        public string Description
        {
            get => (string)this.GetValue(DescriptionProperty);
            set => this.SetValue(DescriptionProperty, value);
        }

        public string Title
        {
            get => (string)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }
    }
}