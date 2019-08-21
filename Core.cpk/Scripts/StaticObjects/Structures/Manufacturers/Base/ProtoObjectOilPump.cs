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

    public abstract class ProtoObjectOilPump : ProtoObjectExtractor
    {
        public const string ErrorRequiresOilSeep = "The pump requires an oil seep.";

        private static readonly ConstructionTileRequirements.Validator ValidatorForPvP
            = new ConstructionTileRequirements.Validator(
                ErrorRequiresOilSeep,
                c =>
                {
                    if (PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
                    {
                        // don't validate this condition on PvE servers
                        return true;
                    }

                    foreach (var obj in c.Tile.StaticObjects)
                    {
                        if (obj.ProtoStaticWorldObject is ObjectDepositOilSeep)
                        {
                            return true;
                        }
                    }

                    return false;
                });

        private static readonly ConstructionTileRequirements.Validator ValidatorForPvE
            = new ConstructionTileRequirements.Validator(
                () => "[b]"
                      + ConstructionTileRequirements.Error_UnsuitableGround_Title
                      + "[/b]"
                      + "[br]"
                      + ConstructionTileRequirements.Error_UnsuitableGround_Message_CanBuildOnlyOn
                      + "[*]"
                      + Api.GetProtoEntity<TileBarren>().Name
                      + "[*]"
                      + Api.GetProtoEntity<TileSwamp>().Name,
                c =>
                {
                    if (!PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
                    {
                        // don't validate this condition on PvP servers
                        return true;
                    }

                    var protoTile = c.Tile.ProtoTile;
                    return protoTile is TileBarren
                           || protoTile is TileSwamp;
                });

        public override byte ContainerInputSlotsCount => 1;

        public override byte ContainerOutputSlotsCount => 1;

        protected override BaseUserControlWithWindow ClientOpenUI(ClientObjectData data)
        {
            var objectDeposit = data.GameObject.OccupiedTile.StaticObjects.FirstOrDefault(
                o => o.ProtoStaticWorldObject is ObjectDepositOilSeep);

            var privateState = data.PrivateState;
            return WindowOilPump.Open(
                new ViewModelWindowOilPump(
                    data.GameObject,
                    objectDeposit,
                    privateState,
                    this.ManufacturingConfig,
                    privateState.LiquidContainerState,
                    this.LiquidContainerConfig));
        }

        protected sealed override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            // Oil pump requires each tile to contain an oil seep.
            // Notice: technically it will be possible to construct one oil pump on two oil seeps if they're nearby,
            // so there should be the oil spawn limitation to avoid that case!
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
                              o => o.PhysicsBody.IsStatic
                                   && !(o.PhysicsBody.AssociatedWorldObject?.ProtoWorldObject
                                            is ObjectDepositOilSeep)))
                .Add(ConstructionTileRequirements.ErrorNoFreeSpace,
                     c => c.Tile.StaticObjects.All(o => o.ProtoWorldObject is ObjectDepositOilSeep))
                .Add(ConstructionTileRequirements.ValidatorNotRestrictedArea)
                .Add(LandClaimSystem.ValidatorIsOwnedOrFreeArea);

            this.PrepareConstructionConfig(build,
                                           repair,
                                           out category);
        }

        protected abstract void PrepareConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            out ProtoStructureCategory category);

        protected override IStaticWorldObject SharedGetDepositWorldObject(Tile tile)
        {
            return tile.StaticObjects.FirstOrDefault(
                o => o.ProtoStaticWorldObject is ObjectDepositOilSeep);
        }
    }
}