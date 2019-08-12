namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Bushes
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectBushWaterbulb : ProtoObjectBush
    {
        public const string ErrorNoFruit = "No fruit!";

        public override string Name => "Waterbulb plant";

        protected override string InteractionFailedNoFruitsMessage => ErrorNoFruit;

        protected override TimeSpan TimeToGiveHarvest => TimeSpan.FromHours(1);

        protected override TimeSpan TimeToMature => TimeSpan.FromHours(2);

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var publicState = data.PublicState;
            var clientState = data.ClientState;
            var sceneObject = Client.Scene.GetSceneObject(data.GameObject);

            clientState.RendererLight = ClientLighting.CreateLightSourceSpot(
                sceneObject,
                color: LightColors.BioLuminescenceCold,
                size: 10,
                logicalSize: 5.5,
                positionOffset: (0.5, 0.9));

            void RefreshLightSourceSpot()
            {
                // display light only if the bush has fruit
                clientState.RendererLight.IsEnabled = publicState.HasHarvest;
            }

            publicState.ClientSubscribe(
                _ => _.HasHarvest,
                _ => RefreshLightSourceSpot(),
                data.ClientState);

            RefreshLightSourceSpot();
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
                columns: 5,
                rows: 1);
        }

        protected override void PrepareGatheringDroplist(DropItemsList droplist)
        {
            droplist
                .Add<ItemWaterbulb>(count: 2, countRandom: 1)
                .Add<ItemWaterbulb>(count: 1, probability: 1 / 5.0, condition: SkillForaging.ConditionAdditionalYield);
        }
    }
}