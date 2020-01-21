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

    public class ObjectStoveElectric : ProtoObjectManufacturer
    {
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

            publicState.ClientSubscribe(_ => _.IsActive,
                                        _ => RefreshActiveState(),
                                        data.ClientState);

            RefreshActiveState();

            void RefreshActiveState()
            {
                clientState.Renderer.TextureResource = publicState.IsActive
                                                           ? textureActive
                                                           : this.DefaultTexture;
            }
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
            build.AddStageRequiredItem<ItemIngotSteel>(count: 5);
            build.AddStageRequiredItem<ItemWire>(count: 5);
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
            data.PhysicsBody
                .AddShapeRectangle(size: (1.8, 0.8),
                                   offset: (0.1, 0))
                .AddShapeRectangle(size: (1.6, 1.3),
                                   offset: (0.2, 0),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (1.5, 0.3),
                                   offset: (0.25, 0.85),
                                   group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (1.5, 1.4),
                                   offset: (0.25, 0),
                                   group: CollisionGroups.ClickArea);
        }
    }
}