namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ItemFishPollock : ProtoItemFish
    {
        public override string Description => GetProtoEntity<ItemFishBass>().Description;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override bool IsSaltwaterFish => true;

        public override float MaxLength => 85;

        public override float MaxWeight => 19;

        public override string Name => "Pollock";

        public override byte RequiredFishingKnowledgeLevel => 10;

        protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
        {
            baitWeightList.Add<ItemFishingBaitInsect>(weight: 25)
                          .Add<ItemFishingBaitMix>(weight: 50)
                          .Add<ItemFishingBaitFish>(weight: 25);

            dropItemsList.Add<ItemFishFilletWhite>(count: 2);
        }
    }
}