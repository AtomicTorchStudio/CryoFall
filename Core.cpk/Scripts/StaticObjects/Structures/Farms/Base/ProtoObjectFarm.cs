namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectFarm
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectFarm
        where TPrivateState : StructurePrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public abstract bool IsDrawingPlantShadow { get; }

        public override StaticObjectKind Kind => StaticObjectKind.Floor;

        public override double ObstacleBlockDamageCoef => 0;

        public virtual Vector2D PlacedPlantPositionOffset { get; } = Vector2D.Zero;

        public override void ServerApplyDecay(IStaticWorldObject worldObject, double deltaTime)
        {
            // don't decay if there are any plants on this farm
            foreach (var tile in worldObject.OccupiedTiles)
            {
                foreach (var staticWorldObject in Api.Shared.WrapInTempList(tile.StaticObjects))
                {
                    if (staticWorldObject.ProtoStaticWorldObject is IProtoObjectPlant)
                    {
                        return;
                    }
                }
            }

            base.ServerApplyDecay(worldObject, deltaTime);
        }

        protected sealed override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryFood>();
            this.PrepareFarmConstructionConfig(tileRequirements, build, repair);
        }

        protected abstract void PrepareFarmConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair);
    }
}