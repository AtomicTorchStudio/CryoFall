namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLightFloorLampLarge : ProtoObjectLight
    {
        public override string Description =>
            "Electric floor lamp. Produces extremely powerful white light that can easily illuminate even a large room. Uses disposable batteries.";

        public override Color LightColor => LightColors.ElectricCold;

        public override double LightSize => 22;

        public override string Name => "Large floor lamp";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 800;

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
            build.AddStageRequiredItem<ItemIngotSteel>(count: 2);
            build.AddStageRequiredItem<ItemIngotCopper>(count: 2);
            build.AddStageRequiredItem<ItemGlassRaw>(count: 4);

            repair.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
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
            fuelContainerPrototype = GetProtoEntity<ItemsContainerFuelElectricity>();
        }
    }
}