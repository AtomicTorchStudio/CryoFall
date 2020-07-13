namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ItemFishSalmon : ProtoItemFish
    {
        public override string Description => GetProtoEntity<ItemFishBass>().Description;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override bool IsSaltwaterFish => true;

        public override float MaxLength => 110;

        public override float MaxWeight => 42;

        public override string Name => "Salmon";

        public override byte RequiredFishingKnowledgeLevel => 25;

        protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
        {
            baitWeightList.Add<ItemFishingBaitInsect>(weight: 25)
                          .Add<ItemFishingBaitMix>(weight: 50)
                          .Add<ItemFishingBaitFish>(weight: 25);

            dropItemsList.Add<ItemFishFilletRed>(count: 2);
        }
    }
}