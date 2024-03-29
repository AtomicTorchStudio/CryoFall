﻿namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
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
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectDisplayCase : ProtoObjectDisplayCase
    {
        private readonly ITextureResource textureResourceBack;

        private readonly ITextureResource textureResourceFront;

        public ObjectDisplayCase()
        {
            var texturePath = this.GenerateTexturePath();
            this.textureResourceFront = new TextureResource(texturePath + "Front");
            this.textureResourceBack = new TextureResource(texturePath + "Back");
        }

        public override string Description =>
            "Always wanted to open a museum or display all those trophies you've acquired over the years? Now you can!";

        public override bool HasOwnersList => true;

        public override byte ItemsSlotsCount => 1;

        public override string Name => "Display case";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Glass;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 500;

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            base.ClientSetupBlueprint(tile, blueprint);
            blueprint.SpriteRenderer.TextureResource = this.Icon;
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return base.SharedGetObjectCenterWorldOffset(worldObject)
                   + (0, 0.35);
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
            // create sprite renderer for back part
            var backRenderer = Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                this.textureResourceBack);
            this.ClientSetupRenderer(backRenderer);

            base.ClientInitialize(data);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.1;
            renderer.PositionOffset += (0, 0.2);
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryStorage>();

            build.StagesCount = 5;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemPlanks>(count: 5);
            build.AddStageRequiredItem<ItemGlassRaw>(count: 2);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 2);
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
            var yOffset = 0.2;
            data.PhysicsBody
                .AddShapeRectangle((0.8, 0.35), offset: (0.1, yOffset + 0.1))
                .AddShapeRectangle((0.8, 1.5),  offset: (0.1, yOffset),       group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.8, 0.4),  offset: (0.1, yOffset + 0.9), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle((0.8, 1.5),  offset: (0.1, yOffset),       group: CollisionGroups.ClickArea);
        }
    }
}