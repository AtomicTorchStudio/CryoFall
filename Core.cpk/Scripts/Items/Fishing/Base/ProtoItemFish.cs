namespace AtomicTorch.CBND.CoreMod.Items.Fishing.Base
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoItemFish : ProtoItemWithFreshness, IProtoItemFish
    {
        public FishingBaitWeightReadOnlyList BaitWeightList { get; private set; }

        public IReadOnlyDropItemsList DropItemsList { get; private set; }

        public abstract bool IsSaltwaterFish { get; }

        public virtual string ItemUseCaption => ItemUseCaptions.Cut;

        public abstract float MaxLength { get; }

        public abstract float MaxWeight { get; }

        public abstract byte RequiredFishingKnowledgeLevel { get; }

        /// <summary>
        /// Use this method to define additional condition when this fish could be caught.
        /// </summary>
        public virtual bool ServerCanCatch(ICharacter character, Vector2Ushort fishingTilePosition)
        {
            return true;
        }

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            var item = data.Item;
            this.CallServer(_ => _.ServerRemote_Cut(item));
            return true;
        }

        protected override string GenerateIconPath()
        {
            return "Items/Fishing/" + this.GetType().Name;
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);
            hints.Add(ItemHints.CuttingFish);
        }

        protected abstract void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList);

        protected sealed override void PrepareProtoItemWithFreshness()
        {
            base.PrepareProtoItemWithFreshness();

            var dropItemsList = new DropItemsList();
            var baitWeightList = new FishingBaitWeightList();
            this.PrepareProtoItemFish(baitWeightList, dropItemsList);
            this.DropItemsList = dropItemsList.AsReadOnly();
            this.BaitWeightList = baitWeightList.ToReadOnly();
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered,
                            timeInterval: 0.2,
                            clientMaxSendQueueSize: 20)]
        private void ServerRemote_Cut(IItem item)
        {
            var character = ServerRemoteContext.Character;
            this.ServerValidateItemForRemoteCall(item, character);

            var createItemResult = this.DropItemsList.TryDropToCharacter(character,
                                                                         new DropItemContext(character));
            if (!createItemResult.IsEverythingCreated)
            {
                createItemResult.Rollback();
                return;
            }

            Logger.Important(character + " cut " + item);

            this.ServerNotifyItemUsed(character, item);
            // decrease item count
            Server.Items.SetCount(item, (ushort)(item.Count - 1));

            NotificationSystem.ServerSendItemsNotification(character, createItemResult);
        }
    }
}