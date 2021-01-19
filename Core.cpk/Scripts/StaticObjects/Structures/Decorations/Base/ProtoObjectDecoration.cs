namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using AtomicTorch.CBND.CoreMod.Systems.Construction;

    public abstract class ProtoObjectDecoration
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
            <TPrivateState,
                TPublicState,
                TClientState>
        where TPrivateState : StructurePrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        public override bool IsRelocatable => true;

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

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

    public abstract class ProtoObjectDecoration
        : ProtoObjectDecoration<
            StructurePrivateState,
            StaticObjectPublicState,
            StaticObjectClientState>
    {
    }
}