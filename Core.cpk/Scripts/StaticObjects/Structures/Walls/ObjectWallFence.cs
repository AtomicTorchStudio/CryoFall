namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ObjectWallFence : ProtoObjectWall
    {
        public override string Description =>
            "This metal fence will block movement, but will also allow bullets to fly through and could be a good choice as a strategic defense. Not very durable.";

        public override bool IsActivatesRaidblockOnDestroy => false; // it's too easy/fast to destroy

        public override string Name => "Chain-link fence";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 0.5; // except for ranged weapons (see SharedIsObstacle)

        public override double StructureExplosiveDefenseCoef => 0;

        public override float StructurePointsMax => 2500;

        public override bool SharedIsObstacle(IWorldObject targetObject, IProtoItemWeapon protoWeapon)
        {
            return protoWeapon is not IProtoItemWeaponRanged
                   || protoWeapon is IProtoItemWeaponGrenadeLauncher;
        }

        protected override void PrepareConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryBuildings>();

            build.StagesCount = 3;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotIron>(count: 1);

            repair.StagesCount = 3;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotIron>(count: 1);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Default);
        }
    }
}