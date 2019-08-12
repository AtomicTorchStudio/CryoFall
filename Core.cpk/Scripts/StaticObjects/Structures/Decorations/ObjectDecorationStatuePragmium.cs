namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    internal class ObjectDecorationStatuePragmium : ProtoObjectDecoration
    {
        public override string Description =>
            "Wasting this precious crystal purely for aesthetic purposes is a good demonstration of one's wealth.";

        public override string Name => "Pragmium statue";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 1.0;

        public override float StructurePointsMax => 1000;

        protected BaseClientComponentLightSource ClientCreateLightSource(IClientSceneObject sceneObject)
        {
            return ClientLighting.CreateLightSourceSpot(
                sceneObject,
                color: LightColors.PragmiumLuminescenceSource.WithAlpha(0xAA),
                size: (5, 11),
                positionOffset: (0.5, 1.3));
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            data.ClientState.RendererLight = this.ClientCreateLightSource(
                Client.Scene.GetSceneObject(data.GameObject));
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset = (0.5, 0.2);
            renderer.DrawOrderOffsetY = 0.25;
        }

        protected override void PrepareDecorationConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemOrePragmium>(count: 10);
            build.AddStageRequiredItem<ItemSolvent>(count: 5);
            build.AddStageRequiredItem<ItemIngotSteel>(count: 5);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemOrePragmium>(count: 1);
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((0.8, 0.5), offset: (0.1, 0.3))
                .AddShapeRectangle((0.5, 0.4), offset: (0.25, 0.9), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.5, 0.3), offset: (0.25, 1.0), group: CollisionGroups.HitboxRanged);
        }
    }
}