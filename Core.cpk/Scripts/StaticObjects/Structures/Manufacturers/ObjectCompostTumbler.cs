namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectCompostTumbler : ProtoObjectMulchbox
    {
        public override byte ContainerInputSlotsCount => 4;

        public override byte ContainerOutputSlotsCount => 4;

        public override ElectricityThresholdsPreset DefaultConsumerElectricityThresholds
            => new(startupPercent: 30,
                   shutdownPercent: 20);

        public override string Description =>
            "Allows efficient decomposition of organic matter in a controlled environment to create fertilizer quickly.";

        public override double ElectricityConsumptionPerSecondWhenActive => 0.25;

        public override double ManufacturingSpeedMultiplier => 5;

        public override string Name => "Compost tumbler";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override ushort OrganicCapacity => 1000;

        public override float StructurePointsMax => 3000;

        protected override TextureResource InfoTexture => new("Misc/UI/CompostTumblerInfo");

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var publicState = data.PublicState;
            var sceneObject = data.GameObject.ClientSceneObject;

            var soundEmitter = this.ClientCreateActiveStateSoundEmitterComponent(data.GameObject);
            soundEmitter.Volume = 0.35f;
            soundEmitter.Radius = 1.5f;
            soundEmitter.CustomMaxDistance = 3.5f;

            var componentVibration = sceneObject
                .AddComponent<ClientComponentWorldObjectVibration>();

            componentVibration.Setup(data.ClientState.Renderer,
                                     amplitude: 0.9 / 256.0,
                                     speed: 0.667,
                                     verticalStartOffsetRelative: 0.02);

            publicState.ClientSubscribe(_ => _.IsActive,
                                        _ => RefreshActiveState(),
                                        data.ClientState);

            RefreshActiveState();

            void RefreshActiveState()
            {
                componentVibration.IsEnabled = publicState.IsActive;
                soundEmitter.IsEnabled = publicState.IsActive;
            }
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 1;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryFood>();

            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotCopper>(count: 2);
            build.AddStageRequiredItem<ItemWire>(count: 5);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);

            repair.StagesCount = 5;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 1);
            repair.AddStageRequiredItem<ItemWire>(count: 2);
            repair.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);
        }

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            return base.PrepareSoundPresetObject().Clone()
                       .Replace(ObjectSound.Active, "Objects/Structures/" + this.GetType().Name + "/Active");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (1.8, 1.1), offset: (0.1, 0.1))
                .AddShapeRectangle(size: (1.6, 1.6), offset: (0.2, 0.2),  group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (1.6, 0.3), offset: (0.2, 0.95), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (1.6, 1.7), offset: (0.2, 0.1),  group: CollisionGroups.ClickArea);
        }
    }
}