namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Decorations
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class ProtoObjectDecorationFloor : ProtoObjectDecoration
    {
        public const string ErrorHasFloorDecoration = "There is already floor decoration.";

        public static readonly ConstructionTileRequirements.Validator ValidatorNoFloorDecoration
            = new(ErrorHasFloorDecoration,
                  c => !c.Tile.StaticObjects.Any(
                           o => o.ProtoStaticWorldObject is ProtoObjectDecorationFloor));

        public override bool IsRelocatable => true;

        // it's technically a floor decoration but we don't have a special object kind for it
        // floor works fine
        public override StaticObjectKind Kind => StaticObjectKind.Floor;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrder = DrawOrder.Floor + 1;
        }

        protected sealed override void PrepareDecorationConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            tileRequirements
                .Clear()
                .Add(ConstructionTileRequirements.BasicRequirements)
                .Add(ConstructionTileRequirements.ValidatorNoStaticObjectsExceptPlayersStructures)
                .Add(ConstructionTileRequirements.ValidatorNoFarmPlot)
                .Add(ValidatorNoFloorDecoration)
                .Add(ConstructionTileRequirements.ValidatorNotRestrictedArea)
                .Add(ConstructionTileRequirements.ValidatorNoNpcsAround)
                .Add(ConstructionTileRequirements.ValidatorNoPlayersNearby)
                .Add(LandClaimSystem.ValidatorIsOwnedOrFreeLand)
                .Add(LandClaimSystem.ValidatorNoRaid)
                .Add(LandClaimSystem.ValidatorNoShieldProtection);

            this.PrepareFloorDecorationConstructionConfig(build, repair);
        }

        protected abstract void PrepareFloorDecorationConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair);

        // no physics for floor decoration
        protected sealed override void SharedCreatePhysics(CreatePhysicsData data)
        {
        }
    }
}