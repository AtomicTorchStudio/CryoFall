namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WindowPersonalStatistics : BaseUserControlWithWindow
    {
        private Grid gridStatistics;

        private ViewModelPersonalStatistics viewModel;

        protected override void InitControlWithWindow()
        {
            base.InitControlWithWindow();
            this.Window.IsCached = false;

            this.gridStatistics = this.GetByName<Grid>("GridStatistics");
        }

        protected override void WindowClosing()
        {
            base.WindowClosing();
            this.gridStatistics.Children.Clear();
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        protected override void WindowOpening()
        {
            base.WindowOpening();
            this.viewModel = new ViewModelPersonalStatistics();

            var children = this.gridStatistics.Children;
            children.Clear();

            var rowDefinitions = this.gridStatistics.RowDefinitions;
            rowDefinitions.Clear();

            for (var index = 0; index < this.viewModel.Entries.Count; index++)
            {
                rowDefinitions.Add(new RowDefinition());

                var viewModelEntry = this.viewModel.Entries[index];
                var textBlockName = new TextBlock() { Text = viewModelEntry.Name };
                Grid.SetRow(textBlockName, index);
                children.Add(textBlockName);

                var textBlockValue = new TextBlock();
                BindingOperations.SetBinding(
                    textBlockValue,
                    TextBlock.TextProperty,
                    new Binding(nameof(ViewModelPersonalStatistics.IViewModelPersonalStatisticsEntry
                                                                  .ValueText))
                    {
                        Source = viewModelEntry,
                        Mode = BindingMode.OneWay
                    });

                Grid.SetRow(textBlockValue, index);
                Grid.SetColumn(textBlockValue, 2);
                children.Add(textBlockValue);

                var textBlockDash = new TextBlock() { Text = "—", HorizontalAlignment = HorizontalAlignment.Center };
                Grid.SetRow(textBlockDash, index);
                Grid.SetColumn(textBlockDash, 1);
                children.Add(textBlockDash);
            }
        }
    }
}