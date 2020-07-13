namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ItemFishCarp : ProtoItemFish
    {
        public override string Description => GetProtoEntity<ItemFishBass>().Description;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override bool IsSaltwaterFish => false;

        public override float MaxLength => 78;

        public override float MaxWeight => 8;

        public override string Name => "Carp";

        public override byte RequiredFishingKnowledgeLevel => 15;

        protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
        {
            baitWeightList.Add<ItemFishingBaitInsect>(weight: 30)
                          .Add<ItemFishingBaitMix>(weight: 50)
                          .Add<ItemFishingBaitFish>(weight: 20);

            dropItemsList.Add<ItemFishFilletRed>(count: 2);
        }
    }
}