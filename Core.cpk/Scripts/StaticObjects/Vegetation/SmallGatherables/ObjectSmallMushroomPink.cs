namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.SmallGatherables
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectSmallMushroomPink : ProtoObjectSmallGatherableVegetation
    {
        public override string Name => "Pink mushroom";

        protected override TimeSpan TimeToMature => TimeSpan.FromHours(1);

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var publicState = data.SyncPublicState;
            var clientState = data.ClientState;
            var sceneObject = Client.Scene.GetSceneObject(data.GameObject);

            clientState.RendererLight = ClientLighting.CreateLightSourceSpot(
                sceneObject,
                color: LightColors.BioLuminescencePink,
                size: 5,
                logicalSize: 2.5,
                positionOffset: (0.5, 0.5));

            void RefreshLightSourceSpot()
            {
                // display light only if the bush has fruit
                clientState.RendererLight.IsEnabled = publicState.IsFullGrown(this);
            }

            publicState.ClientSubscribe(
                _ => _.GrowthStage,
                _ => RefreshLightSourceSpot(),
                data.ClientState);

            RefreshLightSourceSpot();
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.33;
        }

        protected override void ClientUpdate(ClientUpdateData data)
        {
            base.ClientUpdate(data);

            // update light opacity accordingly to the time of day
            data.ClientState.RendererLight.Opacity
                = Math.Min(1,
                           ClientTimeOfDayVisualComponent.CurrentNightFraction
                           + ClientTimeOfDayVisualComponent.CurrentDuskDawnFraction);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureAtlasResource(
                base.PrepareDefaultTexture(thisType),
                columns: 3,
                rows: 1);
        }

        protected override void PrepareGatheringDroplist(DropItemsList droplist)
        {
            droplist
                .Add<ItemMushroomPink>(count: 1)
                .Add<ItemMushroomPink>(count: 1,
                                       probability: 1 / 5.0,
                                       condition: SkillForaging.ConditionAdditionalYield);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(
                    radius: 0.15,
                    center: (0.5, 0.33))
                .AddShapeRectangle(
                    offset: (0.25, 0.2),
                    size: (0.5, 0.6),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    offset: (0.25, 0.2),
                    size: (0.5, 0.6),
                    group: CollisionGroups.HitboxRanged)
                .AddShapeCircle(
                    radius: 0.45,
                    center: (0.5, 0.5),
                    group: CollisionGroups.ClickArea);
        }
    }
}