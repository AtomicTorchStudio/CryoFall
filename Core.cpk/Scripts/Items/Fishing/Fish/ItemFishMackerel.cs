namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ItemFishMackerel : ProtoItemFish
    {
        public override string Description => GetProtoEntity<ItemFishBass>().Description;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override bool IsSaltwaterFish => true;

        public override float MaxLength => 80;

        public override float MaxWeight => 28;

        public override string Name => "Mackerel";

        public override byte RequiredFishingKnowledgeLevel => 45;

        protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
        {
            baitWeightList.Add<ItemFishingBaitInsect>(weight: 5)
                          .Add<ItemFishingBaitMix>(weight: 45)
                          .Add<ItemFishingBaitFish>(weight: 50);

            dropItemsList.Add<ItemFishFilletWhite>(count: 3);
        }
    }
}