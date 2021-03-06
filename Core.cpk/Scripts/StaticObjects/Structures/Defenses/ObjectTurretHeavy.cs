﻿namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Defenses
{
    using AtomicTorch.CBND.CoreMod.Characters.Turrets;
    using AtomicTorch.CBND.CoreMod.ItemContainers.Turrets;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ObjectTurretHeavy : ProtoObjectTurret
    {
        public override BaseItemsContainerTurretAmmo ContainerAmmoType
            => Api.GetProtoEntity<ContainerTurretHeavyAmmo>();

        public override string Description =>
            "Fully automated heavy sentry turret.";

        public override double ElectricityConsumptionPerSecondWhenActive => 0.25;

        public override string Name => "Heavy turret";

        public override double StructureExplosiveDefenseCoef => 0.5;

        public override float StructurePointsMax => 2800;

        protected override void PrepareConstructionConfigTurret(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade)
        {
            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 4);
            build.AddStageRequiredItem<ItemWire>(count: 4);
            build.AddStageRequiredItem<ItemComponentsHighTech>(count: 1);
            build.AddStageRequiredItem<ItemComponentsWeapon>(count: 1);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 4);
            repair.AddStageRequiredItem<ItemWire>(count: 4);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier3);
        }

        protected override void PrepareProtoTurretObject(out IProtoCharacterTurret protoCharacter)
        {
            protoCharacter = GetProtoEntity<CharacterTurretHeavy>();
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.9, 1.17), offset: (0.05, 0), group: CollisionGroups.ClickArea);
        }
    }
}