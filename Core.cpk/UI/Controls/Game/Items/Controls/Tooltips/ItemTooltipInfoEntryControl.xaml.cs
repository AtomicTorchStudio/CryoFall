namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using System.Windows;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemTooltipInfoEntryControl : BaseUserControl
    {
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title),
                                        typeof(string),
                                        typeof(ItemTooltipInfoEntryControl),
                                        new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value",
            typeof(string),
            typeof(ItemTooltipInfoEntryControl),
            new PropertyMetadata(default(string)));

        public string Title
        {
            get => (string)this.GetValue(TitleProperty);
            set => this.SetValue(TitleProperty, value);
        }

        public string Value
        {
            get => (string)this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        public static UIElement Create(string title, string value)
        {
            return new ItemTooltipInfoEntryControl() { Title = title, Value = value };
        }
    }
}