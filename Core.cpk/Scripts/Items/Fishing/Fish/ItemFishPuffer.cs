namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ItemFishPuffer : ProtoItemFish
    {
        public override string Description => GetProtoEntity<ItemFishBass>().Description;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override bool IsSaltwaterFish => true;

        public override float MaxLength => 28;

        public override float MaxWeight => 1.5f;

        public override string Name => "Puffer";

        public override byte RequiredFishingKnowledgeLevel => 0;

        protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
        {
            baitWeightList.Add<ItemFishingBaitInsect>(weight: 40)
                          .Add<ItemFishingBaitMix>(weight: 55)
                          .Add<ItemFishingBaitFish>(weight: 5);

            dropItemsList.Add<ItemFishFilletWhite>(count: 1);
        }
    }
}