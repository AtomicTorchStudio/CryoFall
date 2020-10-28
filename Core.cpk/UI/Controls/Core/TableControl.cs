namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class TableControl : BaseControl
    {
        public static readonly DependencyProperty ColumnsSpacingWidthProperty =
            DependencyProperty.Register(
                nameof(ColumnsSpacingWidth),
                typeof(double),
                typeof(TableControl),
                new PropertyMetadata(10.0));

        private UIElementCollection children;

        private int currentRowsCount;

        private Grid grid;

        private RowDefinitionCollection rows;

        static TableControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TableControl),
                new FrameworkPropertyMetadata(typeof(TableControl)));
        }

        public TableControl()
        {
        }

        public double ColumnsSpacingWidth
        {
            get => (double)this.GetValue(ColumnsSpacingWidthProperty);
            set => this.SetValue(ColumnsSpacingWidthProperty, value);
        }

        public bool IsEmpty => this.currentRowsCount == 0;

        public void Add(FrameworkElement controlKey, FrameworkElement controlValue)
        {
            this.rows.Add(new RowDefinition());

            var row = this.currentRowsCount++;
            if (controlKey is not null)
            {
                Grid.SetRow(controlKey, row);
                this.children.Add(controlKey);
            }

            if (controlValue is not null)
            {
                Grid.SetRow(controlValue, row);
                Grid.SetColumn(controlValue, 2);
                this.children.Add(controlValue);
            }
        }

        public void Clear()
        {
            if (this.currentRowsCount == 0)
            {
                return;
            }

            this.rows.Clear();
            this.children.Clear();
            this.currentRowsCount = 0;
        }

        protected override void InitControl()
        {
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            this.grid = templateRoot.GetByName<Grid>("LayoutRoot");
            this.children = this.grid.Children;
            this.rows = this.grid.RowDefinitions;
        }
    }
}