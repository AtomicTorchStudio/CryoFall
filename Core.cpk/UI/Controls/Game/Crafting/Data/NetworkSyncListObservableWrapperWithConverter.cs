namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class NetworkSyncListObservableWrapperWithConverter<TItem, TViewModel> : IDisposable
        where TViewModel : IDisposable
    {
        private readonly Func<TItem, TViewModel> convertFunc;

        private readonly NetworkSyncList<TItem> netList;

        private readonly ObservableCollection<TViewModel> observableCollection;

        private bool isDisposed;

        /// <summary>
        /// Please use <see cref="NetworkSyncListExtensions.ToObservableCollectionWithWrapper{TKey,TViewModel}" /> instead.
        /// </summary>
        public NetworkSyncListObservableWrapperWithConverter(
            NetworkSyncList<TItem> netList,
            Func<TItem, TViewModel> convertFunc)
        {
            this.netList = netList;
            this.convertFunc = convertFunc;
            this.observableCollection = new ObservableCollection<TViewModel>(netList.Select(convertFunc));
            this.EventsSubscribe();
        }

        public ObservableCollection<TViewModel> ObservableCollection => this.observableCollection;

        public void Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            this.isDisposed = true;
            this.EventsUnsubscribe();

            var items = this.observableCollection.Count > 0 ? this.observableCollection.ToList() : null;
            this.observableCollection.Clear();

            if (items != null)
            {
                foreach (var item in items)
                {
                    item.Dispose();
                }
            }
        }

        private void AddOrSetItem(int index, TItem value)
        {
            var currentCount = this.observableCollection.Count;
            while (currentCount < index)
            {
                // add empty items to increase size of list
                this.observableCollection.Add(default);
                currentCount++;
            }

            var addedItem = this.convertFunc(value);
            if (currentCount == index)
            {
                // add to end
                this.observableCollection.Add(addedItem);
                return;
            }

            var existingItem = this.observableCollection[index];
            this.observableCollection[index] = addedItem;
            existingItem?.Dispose();
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
            this.AddOrSetItem(index, value);
        }

        private void NetListClientElementRemovedHandler(NetworkSyncList<TItem> source, int index, TItem removedValue)
        {
            var item = this.observableCollection.ElementAt(index);
            item?.Dispose();
            this.observableCollection.RemoveAt(index);
        }
    }
}