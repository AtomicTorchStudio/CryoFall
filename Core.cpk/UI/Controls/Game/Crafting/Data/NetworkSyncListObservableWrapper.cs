namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using System;
    using System.Collections.ObjectModel;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class NetworkSyncListObservableWrapper<TItem> : IDisposable
    {
        private readonly NetworkSyncList<TItem> netList;

        private readonly ObservableCollection<TItem> observableCollection;

        private bool isDisposed;

        /// <summary>
        /// Please use <see cref="NetworkSyncListExtensions.ToObservableCollection{T}" /> instead.
        /// </summary>
        public NetworkSyncListObservableWrapper(NetworkSyncList<TItem> netList)
        {
            this.netList = netList;
            this.observableCollection = new ObservableCollection<TItem>(netList);
            this.EventsSubscribe();
        }

        public ObservableCollection<TItem> ObservableCollection => this.observableCollection;

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
            this.EventsUnsubscribe();
        }

        private void EventsSubscribe()
        {
            this.netList.ClientElementInserted += this.NetListClientElementInsertedHandler;
            this.netList.ClientElementRemoved += this.NetListClientElementRemovedHandler;
        }

        private void EventsUnsubscribe()
        {
            this.netList.ClientElementInserted -= this.NetListClientElementInsertedHandler;
            this.netList.ClientElementRemoved -= this.NetListClientElementRemovedHandler;
        }

        private void NetListClientElementInsertedHandler(NetworkSyncList<TItem> source, int index, TItem value)
        {
            this.observableCollection[index] = value;
        }

        private void NetListClientElementRemovedHandler(NetworkSyncList<TItem> source, int index, TItem removedValue)
        {
            this.observableCollection.RemoveAt(index);
        }
    }
}