namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class MobSnakeGreen : ProtoCharacterMob
    {
        public override float CharacterWorldHeight => 1.2f;

        public override double MobKillExperienceMultiplier => 1.0;

        public override string Name => "Green snake";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.SoftTissues;

        public override double StatDefaultHealthMax => 50;

        public override double StatMoveSpeed => 1;

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonSnakeGreen>();

            // primary loot
            lootDroplist
                .Add<ItemToxin>(count: 1);
        }

        protected override void ServerInitializeCharacterMob(ServerInitializeData data)
        {
            base.ServerInitializeCharacterMob(data);

            var weaponProto = GetProtoEntity<ItemWeaponSnakeBite>();
            data.PrivateState.WeaponState.SharedSetWeaponProtoOnly(weaponProto);
            data.PublicState.SetCurrentWeaponProtoOnly(weaponProto);
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            var character = data.GameObject;
            var currentStats = data.PublicState.CurrentStats;

            ServerCharacterAiHelper.ProcessAggressiveAi(
                character,
                isRetreating: currentStats.HealthCurrent < currentStats.HealthMax / 3,
                distanceRetreat: 7,
                distanceEnemyTooClose: 1,
                distanceEnemyTooFar: 5,
                out var movementDirection,
                out var rotationAngleRad);

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }
    }
}