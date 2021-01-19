namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientOptions.General;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.GameApi.Logging;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class LogOverlayControl : BaseUserControl
    {
        private readonly List<LogEntryWithOrigin> logEntriesQueue
            = new();

        private bool isAutoScrollToBottom = true;

        private bool isDisplayed;

        private bool isUserScrollChange = true;

        private ScrollViewer scrollViewerLog;

        private ViewModelLogOverlayControl viewModel;

        public static LogOverlayControl Instance { get; private set; }

        public bool IsDisplayed
        {
            get => this.isDisplayed;
            set
            {
                if (this.isDisplayed == value)
                {
                    return;
                }

                this.isDisplayed = value;

                this.Visibility = this.isDisplayed ? Visibility.Visible : Visibility.Collapsed;

                if (!this.isDisplayed)
                {
                    this.Clear();
                }
            }
        }

        public void Clear()
        {
            if (this.logEntriesQueue.Count > 0)
            {
                this.logEntriesQueue.Clear();
            }

            this.viewModel?.LogEntriesCollection.Clear();
        }

        public void Display(LogEntryWithOrigin entry)
        {
            this.logEntriesQueue.Add(entry);
            this.IsDisplayed = true;
            this.AddQueuedEntries(forceScrollToBottom: false);
        }

        public void Display(List<LogEntryWithOrigin> entries)
        {
            if (entries.Count == 0)
            {
                return;
            }

            this.logEntriesQueue.AddRange(entries);
            this.IsDisplayed = true;
            this.AddQueuedEntries(forceScrollToBottom: false);
        }

        protected override void InitControl()
        {
            ConsoleLogHelper.InitBrushes(this);

            var itemsControlLogEntries = this.GetByName<ItemsControl>("ItemsControlLogEntries");
            this.scrollViewerLog =
                (ScrollViewer)VisualTreeHelper.GetChild(
                    VisualTreeHelper.GetChild(itemsControlLogEntries, 0),
                    0);

            Instance = this;
            this.IsDisplayed = false;
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            this.DataContext = this.viewModel = new ViewModelLogOverlayControl();
            this.AddQueuedEntries(forceScrollToBottom: true);

            this.scrollViewerLog.ScrollChanged += this.ScrollViewerLogScrollChangedHandler;
            this.MouseRightButtonUp += this.MouseRightButtonUpHandler;
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();

            this.scrollViewerLog.ScrollChanged -= this.ScrollViewerLogScrollChangedHandler;
            this.MouseRightButtonUp -= this.MouseRightButtonUpHandler;

            this.DataContext = this.viewModel = null;
            this.logEntriesQueue.Clear();
        }

        private void AddQueuedEntries(bool forceScrollToBottom)
        {
            if (!this.isLoaded
                || !this.isDisplayed)
            {
                return;
            }

            if (this.logEntriesQueue.Count == 0)
            {
                return;
            }

            if (!Api.IsEditor
                && !GeneralOptionDeveloperMode.IsEnabled)
            {
                // display overlay log only in the editor or when the developer mode is enabled
                return;
            }

            this.isUserScrollChange = false;

            var list = this.viewModel.LogEntriesCollection;

            // process queue and add each entry to log list
            foreach (var logEntryWithSource in this.logEntriesQueue)
            {
                var logEntry = logEntryWithSource.LogEntry;
                list.Add(
                    ConsoleLogHelper.CreateViewModelLogEntry(
                        logEntry,
                        isFromServer: logEntryWithSource.IsServerLogEntry));
            }

            this.logEntriesQueue.Clear();

            this.ScrollToBottom(force: forceScrollToBottom);

            this.isUserScrollChange = true;
        }

        private void MouseRightButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            this.IsDisplayed = false;
        }

        private void ScrollToBottom(bool force)
        {
            if (force)
            {
                this.isAutoScrollToBottom = true;
            }
            else if (!this.isAutoScrollToBottom)
            {
                return;
            }

            this.isUserScrollChange = false;
            this.scrollViewerLog.ScrollToBottom();
            this.isUserScrollChange = true;
        }

        private void ScrollViewerLogScrollChangedHandler(object sender, ScrollChangedEventArgs e)
        {
            if (!this.isUserScrollChange)
            {
                return;
            }

            // when user scrolled the log
            var verticalOffset = this.scrollViewerLog.VerticalOffset;
            var scrollableHeight = this.scrollViewerLog.ScrollableHeight;
            this.isAutoScrollToBottom = verticalOffset == scrollableHeight;
        }
    }
}