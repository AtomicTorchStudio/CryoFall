namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Signs
{
    using System.Windows;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ObjectSignControl : BaseUserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text),
                                        typeof(string),
                                        typeof(ObjectSignControl),
                                        new PropertyMetadata(defaultValue: "Text"));

        public string Text
        {
            get => (string)this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
    }
};