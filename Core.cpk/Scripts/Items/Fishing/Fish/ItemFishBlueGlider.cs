namespace AtomicTorch.CBND.CoreMod.Items.Fishing
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Events;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// This special pragmium fish could be caught only during an event.
    /// </summary>
    public class ItemFishBlueGlider : ProtoItemFish
    {
        public const int SkillFishingLevelRequired = 10;

        public override string Description => "Unique pragmium-based aquatic lifeform native to this world.";

        public override TimeSpan FreshnessDuration => ExpirationDuration.LongLasting;

        public override bool IsSaltwaterFish => true;

        public override float MaxLength => 100;

        public override float MaxWeight => 15;

        public override string Name => "Blue glider";

        // Please note: it's not used as a different check is performed (character skill level). 
        public override byte RequiredFishingKnowledgeLevel => 0;

        public override bool ServerCanCatch(ICharacter character, Vector2Ushort fishingTilePosition)
        {
            return character.SharedHasSkill<SkillFishing>(SkillFishingLevelRequired)
                   && SharedEventHelper.SharedIsInsideEventArea<EventFishingBlueGlider>(
                       fishingTilePosition.ToVector2D() + (0.5, 0.5));
        }

        protected override void PrepareProtoItemFish(FishingBaitWeightList baitWeightList, DropItemsList dropItemsList)
        {
            baitWeightList.Add<ItemFishingBaitInsect>(weight: 30)
                          .Add<ItemFishingBaitMix>(weight: 40)
                          .Add<ItemFishingBaitFish>(weight: 30);

            dropItemsList.Add<ItemOrePragmium>(count: 2, countRandom: 1)
                         .Add<ItemSlime>(count: 3,         countRandom: 2)
                         .Add<ItemSulfurPowder>(count: 25, countRandom: 25);
        }
    }
}