﻿namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class ItemFishBlackGoby : ProtoItemFish
    {
        public override string Description => GetProtoEntity<ItemFishBass>().Description;

        public override TimeSpan FreshnessDuration => ExpirationDuration.Normal;

        public override bool IsSaltwaterFish => true;

        public override float MaxLength => 15;

        public override float MaxWeight => 1.5f;

        public override string Name => "Black goby";

        public override byte RequiredFishingKnowledgeLevel => 0;

        protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
        {
            baitWeightList.Add<ItemFishingBaitInsect>(weight: 60)
                          .Add<ItemFishingBaitMix>(weight: 35)
                          .Add<ItemFishingBaitFish>(weight: 5);

            dropItemsList.Add<ItemFishFilletWhite>(count: 1);
        }
    }
}