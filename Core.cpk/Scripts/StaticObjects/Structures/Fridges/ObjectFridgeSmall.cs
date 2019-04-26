namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Fridges
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectFridgeSmall : ProtoObjectFridge
    {
        public override string Description =>
            "Convenient way to store perishable food. Fridge will keep it fresh much longer. Uses built in solar power generator.";

        public override byte ItemsSlotsCount => 6;

        public override string Name => "Small fridge";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 250;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            // create sound emitter
            var soundEmitter = Client.Audio.CreateSoundEmitter(
                data.GameObject,
                new SoundResource("Objects/Structures/ObjectFridge/Active"),
                isLooped: true,
                volume: 0.3f,
                radius: 1f);
            soundEmitter.CustomMaxDistance = 3.5f;
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.3;
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryStorage>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 2);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 2);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 2);
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 2);
            repair.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(
                    size: (0.9, 0.6),
                    offset: (0.05, 0))
                .AddShapeRectangle(
                    size: (1, 1),
                    offset: (0, 0),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    size: (1, 1),
                    offset: (0, 0),
                    group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(
                    size: (1, 1.45),
                    offset: (0, 0),
                    group: CollisionGroups.ClickArea);
        }
    }
}