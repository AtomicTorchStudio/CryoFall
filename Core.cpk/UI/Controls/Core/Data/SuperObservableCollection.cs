namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Scripting;
    using JetBrains.Annotations;
    using static SuperObservableCollectionEventArgsCache;

    public class SuperObservableCollection<T> : ObservableCollection<T>
    {
        public SuperObservableCollection()
        {
        }

        public SuperObservableCollection(List<T> list) : base(list)
        {
        }

        public SuperObservableCollection([NotNull] IEnumerable<T> collection)
            : base(collection)
        {
        }

        public new List<T> Items => (List<T>)base.Items;

        public void AddRange(IEnumerable<T> itemsToAdd)
        {
            this.InternalAddRange(itemsToAdd, clearBeforeAdd: false);
        }

        public void ClearAndAddRange(IEnumerable<T> newItems)
        {
            this.InternalAddRange(newItems, clearBeforeAdd: true);
        }

        public void Sort()
        {
            var items = this.Items;
            using (var tempList = Api.Shared.WrapInTempList(items))
            {
                var sorted = tempList.AsList();
                sorted.Sort();

                if (items.SequenceEqual(sorted))
                {
                    // the items are already sorted
                    return;
                }

                items.Clear();
                items.AddRange(sorted);
            }

            // we didn't change the count
            //this.OnPropertyChanged(EventArgsCountPropertyChanged);

            // the element at indexer might have affected
            this.OnPropertyChanged(EventArgsIndexerPropertyChanged);

            // the whole collection is reordered
            this.OnCollectionChanged(EventArgsResetCollection);
        }

        protected override void ClearItems()
        {
            if (this.Count > 0)
            {
                base.ClearItems();
            }
        }

        private void InternalAddRange(IEnumerable<T> itemsToAdd, bool clearBeforeAdd)
        {
            this.CheckReentrancy();

            var items = this.Items;
            if (clearBeforeAdd)
            {
                items.Clear();
            }

            items.AddRange(itemsToAdd);

            this.OnPropertyChanged(EventArgsCountPropertyChanged);
            this.OnPropertyChanged(EventArgsIndexerPropertyChanged);
            // we've attempted to provide a proper collection Add event here
            // but NoesisGUI crashes when we do this
            this.OnCollectionChanged(EventArgsResetCollection);
        }
    }
}