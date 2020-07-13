namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ItemFishTrout : ProtoItemFish
    {
        public override string Description => GetProtoEntity<ItemFishBass>().Description;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override bool IsSaltwaterFish => false;

        public override float MaxLength => 92;

        public override float MaxWeight => 20;

        public override string Name => "Trout";

        public override byte RequiredFishingKnowledgeLevel => 20;

        protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
        {
            baitWeightList.Add<ItemFishingBaitInsect>(weight: 40)
                          .Add<ItemFishingBaitMix>(weight: 40)
                          .Add<ItemFishingBaitFish>(weight: 20);

            dropItemsList.Add<ItemFishFilletRed>(count: 2);
        }
    }
}