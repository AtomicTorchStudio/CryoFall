namespace AtomicTorch.CBND.CoreMod.Items.Fishing.Base
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public abstract class ProtoItemFish : ProtoItemWithFreshness, IProtoItemFish
    {
        protected ProtoItemFish()
        {
            this.Icon = new TextureResource("Items/Fishing/" + this.GetType().Name);
        }

        public FishingBaitWeightReadOnlyList BaitWeightList { get; private set; }

        public IReadOnlyDropItemsList DropItemsList { get; private set; }

        public override ITextureResource Icon { get; }

        public abstract bool IsSaltwaterFish { get; }

        public virtual string ItemUseCaption => ItemUseCaptions.Cut;

        public abstract float MaxLength { get; }

        public abstract float MaxWeight { get; }

        public abstract byte RequiredFishingKnowledgeLevel { get; }

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            var item = data.Item;
            this.CallServer(_ => _.ServerRemote_Cut(item));
            return true;
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

        private void ServerRemote_Cut(IItem item)
        {
            var character = ServerRemoteContext.Character;
            this.ServerValidateItemForRemoteCall(item, character);

            var createItemResult = this.DropItemsList.TryDropToCharacter(character,
                                                                         new DropItemContext(character, null));
            if (!createItemResult.IsEverythingCreated)
            {
                createItemResult.Rollback();
                return;
            }

            Logger.Important(character + " cut " + item);

            this.ServerNotifyItemUsed(character, item);
            // decrease item count
            Server.Items.SetCount(item, (ushort)(item.Count - 1));
        }
    }
}