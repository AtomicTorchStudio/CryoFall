namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class MobStarfish : ProtoCharacterMob
    {
        public override bool AiIsRunAwayFromHeavyVehicles => true;

        public override float CharacterWorldHeight => 0.5f;

        public override double MobKillExperienceMultiplier => 0.1;

        public override string Name => "Starfish";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.SoftTissues;

        public override double ServerUpdateIntervalSeconds => 1;

        public override double StatDefaultHealthMax => 40;

        public override double StatMoveSpeed => 0.1;

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonStarfish>();

            // random loot
            lootDroplist.Add<ItemSlime>(count: 1, countRandom: 1);

            // extra loot
            lootDroplist.Add<ItemSlime>(count: 1, condition: SkillHunting.ServerRollExtraLoot);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            var character = data.GameObject;

            ServerCharacterAiHelper.ProcessRetreatingAi(
                character,
                distanceRetreat: 4,
                out var movementDirection,
                out var rotationAngleRad);

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }
    }
}