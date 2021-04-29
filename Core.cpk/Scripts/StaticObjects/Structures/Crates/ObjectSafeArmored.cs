namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectSafeArmored : ProtoObjectCrate
    {
        protected const double VerticalOffset = 1.0;

        private readonly ITextureResource textureResourceBase;

        private readonly ITextureResource textureResourceSafe;

        public ObjectSafeArmored()
        {
            var texturePath = this.GenerateTexturePath();
            this.textureResourceBase = new TextureResource(texturePath + "Base");
            this.textureResourceSafe = new TextureResource(texturePath);
        }

        public override Vector2Int BlueprintTileOffset => (0, -1);

        public override string Description =>
            "Heavily armored safe that can withstand even the most powerful explosives. Not easy to make, but it can certainly ensure the safety of your valuables.";

        public override bool HasOwnersList => true;

        public override bool IsRelocatable
            => PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false);

        public override byte ItemsSlotsCount => 32;

        public override string Name => "Armored safe";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override double StructureExplosiveDefenseCoef => 0.5;

        public override float StructurePointsMax => 30000;

        protected override Vector2D ItemIconOffset => (0.035 + 0.5, 0.7);

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            base.ClientSetupBlueprint(tile, blueprint);

            var rendererBase = Client.Rendering.CreateSpriteRenderer(blueprint.SceneObject,
                                                                     null);
            rendererBase.RenderingMaterial = blueprint.SpriteRenderer.RenderingMaterial;
            this.ClientSetupRendererBase(rendererBase);
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return base.SharedGetObjectCenterWorldOffset(worldObject)
                   + (0, 0.9);
        }

        protected override ITextureResource ClientCreateIcon()
        {
            return this.textureResourceSafe;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var rendererBase = Client.Rendering.CreateSpriteRenderer(data.GameObject,
                                                                     null);

            this.ClientSetupRendererBase(rendererBase);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset += (0, 0.2 + VerticalOffset);
            renderer.DrawOrderOffsetY = VerticalOffset - 0.7;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("#",
                         "#");
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
            build.StageDurationSeconds = BuildDuration.Medium;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 5);
            build.AddStageRequiredItem<ItemComponentsMechanical>(count: 2);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Medium;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 3);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return this.textureResourceSafe;
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier4);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.8, 0.575), offset: (0.1, 0.3 + VerticalOffset))
                .AddShapeRectangle(size: (0.8, 0.6),
                                   offset: (0.1, 0.4 + VerticalOffset),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.8, 0.3),
                                   offset: (0.1, 1.1 + VerticalOffset),
                                   group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.8, 1.1),
                                   offset: (0.1, 0.2 + VerticalOffset),
                                   group: CollisionGroups.ClickArea);
        }

        private void ClientSetupRendererBase(IComponentSpriteRenderer rendererBase)
        {
            rendererBase.TextureResource = this.textureResourceBase;
            rendererBase.DrawOrder = DrawOrder.Floor + 1;
        }
    }
}