namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ItemFishBass : ProtoItemFish
    {
        public override string Description => "Freshly caught fish must be cleaned first before using them in cooking.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override bool IsSaltwaterFish => false;

        public override float MaxLength => 60;

        public override float MaxWeight => 10;

        public override string Name => "Bass";

        public override byte RequiredFishingKnowledgeLevel => 45;

        protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
        {
            baitWeightList.Add<ItemFishingBaitInsect>(weight: 5)
                          .Add<ItemFishingBaitMix>(weight: 35)
                          .Add<ItemFishingBaitFish>(weight: 60);

            dropItemsList.Add<ItemFishFilletWhite>(count: 2);
        }
    }
}