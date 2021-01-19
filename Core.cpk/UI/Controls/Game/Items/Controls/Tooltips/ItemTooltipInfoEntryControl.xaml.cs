namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ItemTooltipInfoEntryControl : BaseUserControl
    {
        public static readonly DependencyProperty TitleProperty
            = DependencyProperty.Register(nameof(Title),
                                          typeof(string),
                                          typeof(ItemTooltipInfoEntryControl),
                                          new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ValueBrushProperty
            = DependencyProperty.Register("ValueBrush",
                                          typeof(Brush),
                                          typeof(ItemTooltipInfoEntryControl),
                                          new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty ValueProperty
            = DependencyProperty.Register("Value",
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

        public Brush ValueBrush
        {
            get => (Brush)this.GetValue(ValueBrushProperty);
            set => this.SetValue(ValueBrushProperty, value);
        }

        public static ItemTooltipInfoEntryControl Create(string title, string value)
        {
            return new() { Title = title, Value = value };
        }
    }
}