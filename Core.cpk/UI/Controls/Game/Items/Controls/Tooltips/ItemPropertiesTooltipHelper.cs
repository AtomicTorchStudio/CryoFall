namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ItemPropertiesTooltipHelper
    {
        public static UIElement Create(string title, string value)
        {
            var grid = new Grid() { Opacity = 0.7 };
            var colums = grid.ColumnDefinitions;
            var children = grid.Children;

            colums.Add(new ColumnDefinition() { Width = GridLength.Auto, MinWidth = 80 });
            colums.Add(new ColumnDefinition() { Width = new GridLength(5) });
            colums.Add(new ColumnDefinition() { Width = GridLength.Auto });

            var style = Api.Client.UI.GetApplicationResource<Style>("TooltipStatTextBlock");

            var textBlockTitle = new TextBlock()
            {
                Text = title,
                Style = style
            };

            var textBlockValue = new TextBlock()
            {
                Text = value,
                Margin = new Thickness(1),
                Foreground = Api.Client.UI.GetApplicationResource<Brush>("BrushColorAltLabelForeground"),
                Style = style
            };

            Grid.SetColumn(textBlockValue, 2);

            children.Add(textBlockTitle);
            children.Add(textBlockValue);

            return grid;
        }
    }
}