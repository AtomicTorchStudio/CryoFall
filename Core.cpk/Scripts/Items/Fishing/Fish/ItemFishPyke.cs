namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ItemFishPyke : ProtoItemFish
    {
        public override string Description => GetProtoEntity<ItemFishBass>().Description;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override bool IsSaltwaterFish => false;

        public override float MaxLength => 65;

        public override float MaxWeight => 18;

        public override string Name => "Pike";

        public override byte RequiredFishingKnowledgeLevel => 30;

        protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
        {
            baitWeightList.Add<ItemFishingBaitInsect>(weight: 5)
                          .Add<ItemFishingBaitMix>(weight: 35)
                          .Add<ItemFishingBaitFish>(weight: 60);

            dropItemsList.Add<ItemFishFilletWhite>(count: 2);
        }
    }
}