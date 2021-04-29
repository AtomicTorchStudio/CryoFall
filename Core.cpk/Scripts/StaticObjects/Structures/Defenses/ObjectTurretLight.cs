namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses
{
    using AtomicTorch.CBND.CoreMod.Characters.Turrets;
    using AtomicTorch.CBND.CoreMod.ItemContainers.Turrets;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ObjectTurretLight : ProtoObjectTurret
    {
        public override BaseItemsContainerTurretAmmo ContainerAmmoType
            => Api.GetProtoEntity<ContainerTurretLightAmmo>();

        public override string Description =>
            "Fully automated light sentry turret.";

        public override double ElectricityConsumptionPerSecondWhenActive => 0.25;

        public override string Name => "Light turret";

        public override double StructureExplosiveDefenseCoef => 0.5;

        public override float StructurePointsMax => 2000;

        protected override void PrepareConstructionConfigTurret(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade)
        {
            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 3);
            build.AddStageRequiredItem<ItemWire>(count: 3);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);
            build.AddStageRequiredItem<ItemComponentsWeapon>(count: 1);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 3);
            repair.AddStageRequiredItem<ItemWire>(count: 3);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier3);
        }

        protected override void PrepareProtoTurretObject(out IProtoCharacterTurret protoCharacter)
        {
            protoCharacter = GetProtoEntity<CharacterTurretLight>();
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.9, 1.17), offset: (0.05, 0), group: CollisionGroups.ClickArea);
        }
    }
}