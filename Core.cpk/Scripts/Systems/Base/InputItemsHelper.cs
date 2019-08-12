namespace AtomicTorch.CBND.CoreMod.Systems
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class InputItemsHelper
    {
        /// <summary>
        /// Please note - no validations are done!
        /// If the item is missing it will be not destroyed and passed.
        /// </summary>
        public static void ServerDestroyItems(
            ICharacter character,
            IReadOnlyList<ProtoItemWithCount> requiredItems)
        {
            var serverItemsService = Api.Server.Items;
            var itemsChangedCount = new Dictionary<IProtoItem, int>();

            foreach (var requiredItem in requiredItems)
            {
                serverItemsService.DestroyItemsOfType(
                    character,
                    requiredItem.ProtoItem,
                    requiredItem.Count,
                    out var destroyedCount);

                if (destroyedCount > 0)
                {
                    itemsChangedCount[requiredItem.ProtoItem] = -(int)destroyedCount;
                }
            }

            NotificationSystem.ServerSendItemsNotification(character, itemsChangedCount);
        }
    }
}