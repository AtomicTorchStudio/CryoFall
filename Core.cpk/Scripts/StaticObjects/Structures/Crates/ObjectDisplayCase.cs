namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectDisplayCase : ProtoObjectDisplayCase
    {
        private readonly ITextureResource textureResourceBack;

        private readonly ITextureResource textureResourceFront;

        public ObjectDisplayCase()
        {
            this.textureResourceFront = new TextureResource(this.GenerateTexturePath() + "Front");
            this.textureResourceBack = new TextureResource(this.GenerateTexturePath() + "Back");
        }

        public override string Description =>
            "Always wanted to open a museum or display all those trophies you've acquired over the years? Now you can!";

        public override bool HasOwnersList => true;

        public override string Name => "Display case";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Glass;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 500;

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            blueprint.SpriteRenderer.TextureResource = this.Icon;
        }

        protected override ITextureResource ClientCreateIcon()
        {
            return ClientProceduralTextureHelper.CreateComposedTexture(
                "Composed ObjectDisplayCase",
                isTransparent: true,
                isUseCache: true,
                textureResources: new[] { this.textureResourceBack, this.textureResourceFront });
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            var worldObject = data.GameObject;
            var clientState = data.ClientState;
            var itemsContainer = data.SyncPublicState.ItemsContainer;

            // create sprite renderer for back part
            Client.Rendering.CreateSpriteRenderer(
                      worldObject,
                      this.textureResourceBack)
                  .DrawOrderOffsetY = 0.1;

            // create sprite renderer for item
            var rendererShowcaseItem = Client.Rendering.CreateSpriteRenderer(
                worldObject,
                TextureResource.NoTexture,
                positionOffset: (0.5, 1),
                spritePivotPoint: (0.5, 0.5));
            rendererShowcaseItem.DrawOrderOffsetY = -0.9;

            void RefreshShowcaseItem()
            {
                var showcasedItem = itemsContainer.Items.FirstOrDefault();
                rendererShowcaseItem.TextureResource = showcasedItem?.ProtoItem.GroundIcon;
                var isEnabled = showcasedItem != null;
                rendererShowcaseItem.IsEnabled = isEnabled;
                if (!isEnabled)
                {
                    // no item to showcase
                    return;
                }

                rendererShowcaseItem.Scale = 0.35 * showcasedItem.ProtoItem.GroundIconScale;
                // hack to make front renderer displayed on top of item renderer
                clientState.Renderer.IsEnabled = false;
                clientState.Renderer.IsEnabled = true;
            }

            ((IClientItemsContainer)itemsContainer).StateHashChanged += RefreshShowcaseItem;

            // create default sprite renderer (top part) and other base stuff
            base.ClientInitialize(data);

            RefreshShowcaseItem();
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.1;
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
            build.AddStageRequiredItem<ItemPlanks>(count: 5);
            build.AddStageRequiredItem<ItemGlassRaw>(count: 2);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 3);
            repair.AddStageRequiredItem<ItemGlassRaw>(count: 1);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
            => this.textureResourceFront;

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((0.8, 0.35), offset: (0.1, 0.1))
                .AddShapeRectangle((0.8, 1.5),  (0.1, 0), CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.8, 1.5),
                                   (0.1, 0),
                                   group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle((0.8, 1.5),
                                   (0.1, 0),
                                   group: CollisionGroups.ClickArea);
        }
    }
}