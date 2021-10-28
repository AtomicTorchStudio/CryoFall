namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectDecorationBannerFaction
        : ProtoObjectDecoration<
            StructurePrivateState,
            ObjectDecorationBannerFaction.PublicState,
            StaticObjectClientState>
    {
        private static readonly Lazy<RenderingMaterial> LazyRenderingMaterialEmblem
            = IsClient
                  ? new Lazy<RenderingMaterial>(CreateRenderingMaterialEmblem)
                  : null;

        public override string Description =>
            "Unique banner that will display your faction emblem.";

        public override string Name => "Faction banner";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 500;

        public override BoundsInt ViewBoundsExpansion => new(0, 0, 0, maxY: 2);

        public override void ServerOnBuilt(IStaticWorldObject structure, ICharacter byCharacter)
        {
            var faction = FactionSystem.ServerGetFaction(byCharacter);
            if (faction is null)
            {
                // should be impossible
                return;
            }

            GetPublicState(structure).FactionEmblem = Faction.GetPublicState(faction).Emblem;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var emblem = data.PublicState.FactionEmblem;
            if (!SharedFactionEmblemProvider.SharedIsValidEmblem(emblem))
            {
                return;
            }

            const double offsetY = 223 / 256.0;
            var renderer = data.ClientState.Renderer;

            var emblemRenderer = Api.Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                ClientFactionEmblemTextureProvider.GetEmblemTexture(emblem, useCache: true));
            emblemRenderer.Size = (209 / 2.0, 209 / 2.0);
            emblemRenderer.PositionOffset = (0.5, renderer.PositionOffset.Y + offsetY);
            emblemRenderer.DrawOrderOffsetY = renderer.DrawOrderOffsetY - offsetY;
            emblemRenderer.SpritePivotPoint = (0.5, 0);
            emblemRenderer.RenderingMaterial = LazyRenderingMaterialEmblem.Value;
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset += (0.0, 0.29);
            renderer.DrawOrderOffsetY = 0.25;
            renderer.SpritePivotPoint = (0.5, 0);
        }

        protected override void PrepareDecorationConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            tileRequirements.Add(
                FactionSystem.ValidatorPlayerHasFaction);

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemPlanks>(count: 5);
            build.AddStageRequiredItem<ItemThread>(count: 5);
            build.AddStageRequiredItem<ItemPaper>(count: 5);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemPlanks>(count: 5);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((0.5, 0.4), offset: (0.25, 0.3))
                .AddShapeRectangle((0.5, 0.4), offset: (0.25, 0.9), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.5, 0.3), offset: (0.25, 1.0), group: CollisionGroups.HitboxRanged);
        }

        private static RenderingMaterial CreateRenderingMaterialEmblem()
        {
            var maskTextureResource = new TextureResource(
                GenerateTexturePath(typeof(ObjectDecorationBannerFaction)) + "_Mask",
                qualityOffset: ClientFactionEmblemTextureProvider.SpriteQualityOffset);

            var material = RenderingMaterial.Create(
                new EffectResource("DrawWithMask"));
            material.EffectParameters
                    .Set("MaskTextureArray", maskTextureResource);
            return material;
        }

        public class PublicState : StaticObjectPublicState
        {
            [SyncToClient]
            public FactionEmblem FactionEmblem { get; set; }
        }
    }
}