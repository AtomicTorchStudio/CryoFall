namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;

    public class ObjectConstructionSite : ProtoObjectConstructionSite
    {
        public override string Description => string.Empty;

        public override string Name => "Construction site";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 1.0;

        protected override bool ClientIsConstructionOrRepairRequirementsTooltipShouldBeDisplayed(
            ConstructionSitePublicState publicState)
        {
            return ClientComponentObjectInteractionHelper.MouseOverObject == publicState.GameObject;
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            // not constructable from the construction menu
            build.IsAllowed = false;
            category = GetCategory<StructureCategoryOther>();
        }
    }
}