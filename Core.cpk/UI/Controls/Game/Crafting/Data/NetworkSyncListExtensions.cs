namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public static class NetworkSyncListExtensions
    {
        public static NetworkSyncListObservableWrapper<T>
            ToObservableCollection<T>(this NetworkSyncList<T> netList)
        {
            return new NetworkSyncListObservableWrapper<T>(netList);
        }

        public static NetworkSyncListObservableWrapperWithConverter<TKey, TViewModel>
            ToObservableCollectionWithWrapper<TKey, TViewModel>(
                this NetworkSyncList<TKey> netList,
                Func<TKey, TViewModel> func)
            where TViewModel : IDisposable
        {
            return new NetworkSyncListObservableWrapperWithConverter<TKey, TViewModel>(
                netList,
                func);
        }
    }
}