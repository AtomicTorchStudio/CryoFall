namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ItemFishYellowfinTuna : ProtoItemFish
    {
        public override string Description => GetProtoEntity<ItemFishBass>().Description;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override bool IsSaltwaterFish => true;

        public override float MaxLength => 200;

        public override float MaxWeight => 100;

        public override string Name => "Yellowfin tuna";

        public override byte RequiredFishingKnowledgeLevel => 85;

        protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
        {
            baitWeightList.Add<ItemFishingBaitInsect>(weight: 5)
                          .Add<ItemFishingBaitMix>(weight: 30)
                          .Add<ItemFishingBaitFish>(weight: 65);

            dropItemsList.Add<ItemFishFilletRed>(count: 4);
        }
    }
}