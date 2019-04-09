namespace AtomicTorch.CBND.CoreMod.Systems.Notifications
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public struct UINotificationItemRemoteCallData : IRemoteCallParameter
    {
        public readonly Dictionary<IProtoItem, int> ItemsChangedCount;

        public UINotificationItemRemoteCallData(Dictionary<IProtoItem, int> itemsChangedCount)
        {
            this.ItemsChangedCount = itemsChangedCount;
        }
    }
}