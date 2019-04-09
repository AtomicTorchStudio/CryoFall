namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.Systems.Construction;

    public abstract class ProtoObjectDecoration : ProtoObjectStructure
    {
        protected sealed override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryDecorations>();
            build.StagesCount = 1;

            this.PrepareDecorationConstructionConfig(tileRequirements, build, repair);
        }

        protected abstract void PrepareDecorationConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair);
    }
}