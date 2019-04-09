namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls
{
    using System.Windows;
    using System.Windows.Markup;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    [ContentProperty(nameof(SlotContent))]
    public partial class HotbarItemSlotExtensionControl : BaseUserControl
    {
        public static readonly DependencyProperty SlotContentProperty =
            DependencyProperty.Register(
                nameof(SlotContent),
                typeof(FrameworkElement),
                typeof(HotbarItemSlotExtensionControl),
                new PropertyMetadata(default(FrameworkElement)));

        public FrameworkElement SlotContent
        {
            get => (FrameworkElement)this.GetValue(SlotContentProperty);
            set => this.SetValue(SlotContentProperty, value);
        }
    }
}