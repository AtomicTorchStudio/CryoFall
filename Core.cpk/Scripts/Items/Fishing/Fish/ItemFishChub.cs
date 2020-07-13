namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ItemFishChub : ProtoItemFish
    {
        public override string Description => GetProtoEntity<ItemFishBass>().Description;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override bool IsSaltwaterFish => false;

        public override float MaxLength => 60;

        public override float MaxWeight => 8;

        public override string Name => "Chub";

        public override byte RequiredFishingKnowledgeLevel => 0;

        protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
        {
            baitWeightList.Add<ItemFishingBaitInsect>(weight: 85)
                          .Add<ItemFishingBaitMix>(weight: 10)
                          .Add<ItemFishingBaitFish>(weight: 5);

            dropItemsList.Add<ItemFishFilletWhite>(count: 1);
        }
    }
}