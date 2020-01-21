namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Extras.UpdatesHistory
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuUpdatesHistory : BaseUserControl
    {
        public static readonly DependencyProperty EntriesProperty =
            DependencyProperty.Register("Entries",
                                        typeof(IReadOnlyList<UpdatesHistoryEntries.Entry>),
                                        typeof(MenuUpdatesHistory),
                                        new PropertyMetadata(default(IReadOnlyList<UpdatesHistoryEntries.Entry>)));

        private AutoScrollViewer autoScrollViewer;

        public IReadOnlyList<UpdatesHistoryEntries.Entry> Entries
        {
            get => (IReadOnlyList<UpdatesHistoryEntries.Entry>)this.GetValue(EntriesProperty);
            private set => this.SetValue(EntriesProperty, value);
        }

        protected override void InitControl()
        {
            this.Entries = UpdatesHistoryEntries.Entries;

            var scrollViewer = this.GetByName<ScrollViewer>("ScrollViewer");
            this.autoScrollViewer = new AutoScrollViewer(scrollViewer);
        }
    }
}