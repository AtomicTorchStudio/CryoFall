namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public abstract class ProtoObjectLithiumOreExtractor : ProtoObjectExtractor
    {
        public const string ErrorRequiresGeothermalSpring = "The extractor requires a geothermal spring.";

        protected static readonly ConstructionTileRequirements.Validator ValidatorForPvP
            = new ConstructionTileRequirements.Validator(
                ErrorRequiresGeothermalSpring,
                c =>
                {
                    if (PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
                    {
                        // don't validate this condition on PvE servers
                        return true;
                    }

                    foreach (var obj in c.Tile.StaticObjects)
                    {
                        if (obj.ProtoStaticWorldObject is ObjectDepositGeothermalSpring)
                        {
                            return true;
                        }
                    }

                    return false;
                });

        protected static readonly ConstructionTileRequirements.Validator ValidatorForPvE
            = new ConstructionTileRequirements.Validator(
                () => "[b]"
                      + ConstructionTileRequirements.Error_UnsuitableGround_Title
                      + "[/b]"
                      + "[br]"
                      + ConstructionTileRequirements.Error_UnsuitableGround_Message_CanBuildOnlyOn
                      + "[*]"
                      + Api.GetProtoEntity<TileForestTemperate>().Name
                      + "[*]"
                      + Api.GetProtoEntity<TileForestBoreal>().Name,
                c =>
                {
                    if (!PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
                    {
                        // don't validate this condition on PvP servers
                        return true;
                    }

                    var protoTile = c.Tile.ProtoTile;
                    return protoTile is TileForestTemperate
                           || protoTile is TileForestBoreal;
                });

        public override byte ContainerInputSlotsCount => 0;

        public override byte ContainerOutputSlotsCount => 1;

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            // Lithium salt extractor requires each tile to contain a geothermal spring.
            tileRequirements
                .Clear()
                .Add(ValidatorForPvP)
                .Add(ValidatorForPvE)
                .Add(ConstructionTileRequirements.BasicRequirements)
                .Add(ConstructionTileRequirements.ValidatorClientOnlyNoCurrentPlayer)
                .Add(ConstructionTileRequirements.ValidatorNoPhysicsBodyDynamic)
                .Add(ConstructionTileRequirements.ErrorNoFreeSpace,
                     c => !ConstructionTileRequirements.TileHasAnyPhysicsObjectsWhere(
                              c.Tile,
                              t => t.PhysicsBody.IsStatic
                                   && !(t.PhysicsBody.AssociatedWorldObject?.ProtoWorldObject is
                                            ObjectDepositGeothermalSpring)))
                .Add(ConstructionTileRequirements.ErrorNoFreeSpace,
                     c => c.Tile.StaticObjects.All(o => o.ProtoWorldObject is ObjectDepositGeothermalSpring))
                .Add(ConstructionTileRequirements.ValidatorNotRestrictedArea)
                .Add(LandClaimSystem.ValidatorIsOwnedOrFreeArea);

            this.PrepareConstructionConfig(build, repair, out category);
        }

        protected abstract void PrepareConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            out ProtoStructureCategory category);

        protected override IStaticWorldObject SharedGetDepositWorldObject(Tile tile)
        {
            return tile.StaticObjects.FirstOrDefault(
                o => o.ProtoStaticWorldObject is ObjectDepositGeothermalSpring);
        }

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            var objectDeposit = data.GameObject.OccupiedTile.StaticObjects.FirstOrDefault(
                o => o.ProtoStaticWorldObject is ObjectDepositGeothermalSpring);

            return WindowLithiumOreExtractor.Open(
                new ViewModelWindowLithiumOreExtractor(
                    data.GameObject,
                    objectDeposit,
                    data.PrivateState,
                    this.ManufacturingConfig,
                    data.PrivateState.LiquidContainerState,
                    this.LiquidContainerConfig));
        }
    }
}