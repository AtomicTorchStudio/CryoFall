namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectStoveElectric : ProtoObjectManufacturer
    {
        private const float HorizontalOffset = 0.215f;

        private static TextureResource textureActive;

        public override byte ContainerFuelSlotsCount => 0;

        public override byte ContainerInputSlotsCount => 8;

        public override byte ContainerOutputSlotsCount => 4;

        public override string Description =>
            "Electric stove allows for faster and more convenient preparation of food.";

        public override double ElectricityConsumptionPerSecondWhenActive => 2;

        public override bool IsAutoSelectRecipe => false;

        public override bool IsFuelProduceByproducts => false;

        public override string Name => "Electric cooking stove";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 2000;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var publicState = data.PublicState;
            var clientState = data.ClientState;

            var soundEmitter = this.ClientCreateActiveStateSoundEmitterComponent(data.GameObject);
            soundEmitter.CustomMaxDistance = 4;
            soundEmitter.Volume = 0.5f;

            publicState.ClientSubscribe(_ => _.IsActive,
                                        _ => RefreshActiveState(),
                                        data.ClientState);

            RefreshActiveState();

            void RefreshActiveState()
            {
                clientState.Renderer.TextureResource = publicState.IsActive
                                                           ? textureActive
                                                           : this.DefaultTexture;

                soundEmitter.IsEnabled = publicState.IsActive;
            }
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.25;
            renderer.PositionOffset = (HorizontalOffset - 0.045, 0);
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##");
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
            build.AddStageRequiredItem<ItemIngotSteel>(count: 2);
            build.AddStageRequiredItem<ItemWire>(count: 2);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var texturePath = GenerateTexturePath(thisType);
            textureActive = new TextureResource(texturePath + "Active");
            return new TextureResource(texturePath);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            var offsetX = HorizontalOffset;
            data.PhysicsBody
                .AddShapeRectangle(size: (1.57, 0.8), offset: (offsetX, 0))
                .AddShapeRectangle(size: (1.57, 0.3), offset: (offsetX, 0.6), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (1.57, 1.4), offset: (offsetX, 0),   group: CollisionGroups.ClickArea);
        }
    }
}