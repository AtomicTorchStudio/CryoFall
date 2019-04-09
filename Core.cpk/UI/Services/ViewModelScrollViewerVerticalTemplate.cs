namespace AtomicTorch.CBND.CoreMod.UI.Services
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelScrollViewerVerticalTemplate : BaseViewModel
    {
        private readonly ScrollViewer scrollViewer;

        public ViewModelScrollViewerVerticalTemplate(ScrollViewer scrollViewer)
        {
            this.scrollViewer = scrollViewer;
            this.scrollViewer.PreviewMouseWheel += this.PreviewMouseWheelHandler;
            this.scrollViewer.ScrollChanged += this.ScrollChangedHandler;
            this.CommandScrollUp = new ActionCommand(this.ExecuteCommandScrollLeft);
            this.CommandScrollDown = new ActionCommand(this.ExecuteCommandScrollRight);
            this.Refresh();
        }

        public BaseCommand CommandScrollDown { get; }

        public BaseCommand CommandScrollUp { get; }

        public Visibility VisibilityScrollDown { get; private set; }

        public Visibility VisibilityScrollUp { get; private set; }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.scrollViewer.PreviewMouseWheel -= this.PreviewMouseWheelHandler;
            this.scrollViewer.ScrollChanged -= this.ScrollChangedHandler;
        }

        private void ExecuteCommandScrollLeft()
        {
            this.scrollViewer.LineUp();
        }

        private void ExecuteCommandScrollRight()
        {
            this.scrollViewer.LineDown();
        }

        private void PreviewMouseWheelHandler(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var scrollviewer = (ScrollViewer)sender;
            if (e.Delta > 0)
            {
                scrollviewer.LineUp();
            }
            else
            {
                scrollviewer.LineDown();
            }

            this.Refresh();
        }

        private void Refresh()
        {
            var scrollableHeight = this.scrollViewer.ScrollableHeight;
            if (this.scrollViewer.VerticalOffset > scrollableHeight
                && scrollableHeight > 0)
            {
                this.scrollViewer.ScrollToVerticalOffset(scrollableHeight);
            }

            var verticalOffset = this.scrollViewer.VerticalOffset;

            this.VisibilityScrollUp = verticalOffset > 0 ? Visibility.Visible : Visibility.Collapsed;

            this.VisibilityScrollDown = scrollableHeight > 0
                                        && verticalOffset < scrollableHeight
                                            ? Visibility.Visible
                                            : Visibility.Collapsed;
        }

        private void ScrollChangedHandler(object sender, ScrollChangedEventArgs e)
        {
            this.Refresh();
        }
    }
}