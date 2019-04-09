namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLightFloorLampOil : ProtoObjectLight
    {
        public override string Description =>
            "Simple oil-fired floor lamp. Could make any room feel more cozy with the nice yellow glow it produces.";

        public override Color LightColor => LightColors.OilLamp;

        public override double LightSize => 19;

        public override string Name => "Oil-fired floor lamp";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 300;

        protected override BaseClientComponentLightSource ClientCreateLightSource(IClientSceneObject sceneObject)
        {
            var lightSource = base.ClientCreateLightSource(sceneObject);

            // add light flickering
            sceneObject.AddComponent<ClientComponentLightSourceEffectFlickering>()
                       .Setup(lightSource,
                              flickeringPercents: 5,
                              flickeringChangePercentsPerSecond: 33);

            return lightSource;
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.5;
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryLight>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemIngotCopper>(count: 1);
            build.AddStageRequiredItem<ItemGlassRaw>(count: 3);

            repair.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotCopper>(count: 1);
        }

        protected override void PrepareDefense(DefenseDescription defense)
        {
            defense.Set(ObjectDefensePresets.Tier1);
        }

        protected override void PrepareFuelConfig(
            out double fuelCapacity,
            out double fuelAmountInitial,
            out double fuelUsePerSecond,
            out IFuelItemsContainer fuelContainerPrototype)
        {
            fuelCapacity = 2500;
            fuelAmountInitial = 0;
            fuelUsePerSecond = 1;
            fuelContainerPrototype = GetProtoEntity<ItemsContainerFuelOil>();
        }
    }
}