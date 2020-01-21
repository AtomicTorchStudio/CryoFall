namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    /// <summary>
    /// Well is a manufacturer: empty bottle -> bottle with water.
    /// </summary>
    public class ObjectWell : ProtoObjectWell
    {
        private readonly ITextureResource textureResourceBack;

        private readonly ITextureResource textureResourceFront;

        public ObjectWell()
        {
            this.textureResourceBack = new TextureResource(this.GenerateTexturePath() + "Back");
            this.textureResourceFront = new TextureResource(this.GenerateTexturePath() + "Front");
        }

        public override string Description =>
            "Provides inexhaustible source of fresh water. Now you won't ever die from thirst. Doesn't really work in deserts and other rocky places, for some reason...";

        public override string Name => "Well";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 1000;

        public override double WaterCapacity => 50;

        public override double WaterProductionAmountPerSecond => 0.2;

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            blueprint.SpriteRenderer.TextureResource = this.Icon;
        }

        protected override ITextureResource ClientCreateIcon()
        {
            return ClientProceduralTextureHelper.CreateComposedTexture(
                "Composed " + this.Id,
                isTransparent: true,
                isUseCache: true,
                textureResources: new[] { this.textureResourceBack, this.textureResourceFront });
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            Client.Rendering.CreateSpriteRenderer(
                      data.GameObject,
                      this.textureResourceBack)
                  .DrawOrderOffsetY = 1.3;
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.7;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##",
                         "##");
        }

        protected override void PrepareConstructionConfigWell(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryIndustry>();

            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemStone>(count: 10);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemStone>(count: 2);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return this.textureResourceFront;
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.3,  center: (0.75, 0.75))
                .AddShapeCircle(radius: 0.3,  center: (1.25, 0.75))
                .AddShapeCircle(radius: 0.75, center: (1, 1), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (1.2, 0.25), offset: (0.4, 1.2), group: CollisionGroups.HitboxRanged)
                .AddShapeCircle(radius: 0.6, center: (1, 1.1), group: CollisionGroups.ClickArea);
        }
    }
}