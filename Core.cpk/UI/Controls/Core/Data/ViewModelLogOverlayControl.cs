namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data
{
    public class ViewModelLogOverlayControl : BaseViewModel
    {
        private SuperObservableCollection<ViewModelLogEntry> logEntriesCollection
            = new();

        public SuperObservableCollection<ViewModelLogEntry> LogEntriesCollection
        {
            get => this.logEntriesCollection;
            set
            {
                if (this.logEntriesCollection == value)
                {
                    return;
                }

                this.logEntriesCollection = value;
                this.NotifyThisPropertyChanged();
            }
        }
    }
}