namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ItemFishRooster : ProtoItemFish
    {
        public override string Description => GetProtoEntity<ItemFishBass>().Description;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override bool IsSaltwaterFish => true;

        public override float MaxLength => 110;

        public override float MaxWeight => 25;

        public override string Name => "Rooster";

        public override byte RequiredFishingKnowledgeLevel => 55;

        protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
        {
            baitWeightList.Add<ItemFishingBaitInsect>(weight: 5)
                          .Add<ItemFishingBaitMix>(weight: 35)
                          .Add<ItemFishingBaitFish>(weight: 60);

            dropItemsList.Add<ItemFishFilletWhite>(count: 3);
        }
    }
}