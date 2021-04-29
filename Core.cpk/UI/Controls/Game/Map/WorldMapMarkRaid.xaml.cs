namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WorldMapMarkRaid : BaseUserControl
    {
        public static readonly DependencyProperty DescriptionProperty
            = DependencyProperty.Register("Description",
                                          typeof(string),
                                          typeof(WorldMapMarkRaid),
                                          new PropertyMetadata(default(string),
                                                               DescriptionPropertyChangedHandler));

        public string Description
        {
            get => (string)this.GetValue(DescriptionProperty);
            set => this.SetValue(DescriptionProperty, value);
        }

        private static void DescriptionPropertyChangedHandler(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = ((WorldMapMarkRaid)d);
            var textBlock = new FormattedTextBlock() { Text = (string)e.NewValue };
            ToolTipServiceExtend.SetToolTip(control, textBlock);
        }
    }
}