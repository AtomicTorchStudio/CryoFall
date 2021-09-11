namespace AtomicTorch.CBND.CoreMod.UI.Services
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// TODO: replace it with ShareSizeGroup when it's implemented https://www.noesisengine.com/bugs/view.php?id=1552
    /// </summary>
    public static class SharedFirstColumnWidthService
    {
        public static readonly DependencyProperty EnabledProperty =
            DependencyProperty.Register("Enabled",
                                        typeof(bool),
                                        typeof(SharedFirstColumnWidthService),
                                        new PropertyMetadata(false, PropertyChangedHandler));

        // cannot use weak references here as they will be lost immediately due to how C# SDK NoesisGUI works
        private static readonly List<Grid> RegisteredGrids = new();

        private static bool isRefreshScheduled;

        public static bool GetEnabled(FrameworkElement element)
        {
            return (bool)element.GetValue(EnabledProperty);
        }

        public static void Register(Grid grid)
        {
            RegisteredGrids.Add(grid);
            grid.Loaded += GridLoadedHandler; // no memory leak here as the handler method is statiс
            ScheduleRefresh();
        }

        public static void SetEnabled(FrameworkElement element, bool value)
        {
            element.SetValue(EnabledProperty, value);
        }

        private static double GetGridActualWidthFirstColumn(Grid grid)
        {
            // it doesn't work for some reason
            //return grid.ColumnDefinitions[0].ActualWidth;

            // find it manually
            var maxActualWidth = 0.0;

            foreach (FrameworkElement child in grid.Children)
            {
                if (Grid.GetColumn(child) != 0)
                {
                    continue;
                }

                var actualWidth = child.ActualWidth;
                if (actualWidth > maxActualWidth)
                {
                    maxActualWidth = actualWidth;
                }
            }

            return maxActualWidth;
        }

        private static void GridLoadedHandler(object sender, RoutedEventArgs e)
        {
            ScheduleRefresh();
        }

        private static void PropertyChangedHandler(
            DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            if (obj is not Grid grid)
            {
                Api.Logger.Error("Element must be a Grid");
                return;
            }

            //Api.Logger.Dev("Changed for " + obj + ": " + e.NewValue);
            Register(grid);
        }

        private static void Refresh()
        {
            //var result = "Layout: " + Environment.NewLine;
            var maxFirstColumnActualWidth = 0.0;

            for (var index = 0; index < RegisteredGrids.Count; index++)
            {
                var grid = RegisteredGrids[index];
                if (grid.Parent is null)
                {
                    RegisteredGrids.RemoveAt(index);
                    index--;
                    //result += " * lost" + Environment.NewLine;
                    continue;
                }

                if (!IsValid(grid))
                {
                    //result += " * invalid" + Environment.NewLine;
                    continue;
                }

                grid.ColumnDefinitions[0].Width = GridLength.Auto;
                grid.UpdateLayout();

                var actualWidthFirstColumn = GetGridActualWidthFirstColumn(grid);
                //result += " * " + actualWidthFirstColumn.ToString("F0") + Environment.NewLine;

                if (actualWidthFirstColumn > maxFirstColumnActualWidth)
                {
                    maxFirstColumnActualWidth = actualWidthFirstColumn;
                }
            }

            //Api.Logger.Dev(result);

            for (var index = 0; index < RegisteredGrids.Count; index++)
            {
                var grid = RegisteredGrids[index];

                if (IsValid(grid))
                {
                    grid.ColumnDefinitions[0].Width = new GridLength((float)maxFirstColumnActualWidth);
                }
            }

            static bool IsValid(Grid grid) => grid.IsLoaded && grid.IsVisible;
        }

        private static void ScheduleRefresh()
        {
            Refresh();

            if (isRefreshScheduled)
            {
                return;
            }

            isRefreshScheduled = true;
            ClientTimersSystem.AddAction(0,
                                         () =>
                                         {
                                             Refresh();
                                             isRefreshScheduled = false;
                                         });
        }
    }
}