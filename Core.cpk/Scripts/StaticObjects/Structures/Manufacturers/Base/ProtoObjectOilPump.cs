namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using System.ComponentModel;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    public abstract class ProtoObjectOilPump : ProtoObjectExtractor
    {
        public const string ErrorTooCloseToAnotherOilPump
            = "Too close to another oil pump.";

        private static readonly ConstructionTileRequirements.Validator ValidatorGroundTypeOrOilSeep
            = new(() => string.Format("{0}[br]{1}[*]{2}[*]{3}",
                                      ConstructionTileRequirements.ErrorCode.UnsuitableGround_Title
                                                                  .GetDescription(),
                                      ConstructionTileRequirements.ErrorCode.UnsuitableGround_Message_CanBuildOnlyOn
                                                                  .GetDescription(),
                                      Api.GetProtoEntity<TileBarren>().Name,
                                      Api.GetProtoEntity<TileSwamp>().Name),
                  c =>
                  {
                      if (PveSystem.SharedIsPve(true))
                      {
                          // no soil requirement in PvE
                          return true;
                      }

                      foreach (var obj in c.Tile.StaticObjects)
                      {
                          if (obj.ProtoStaticWorldObject is ObjectDepositOilSeep)
                          {
                              return true;
                          }
                      }

                      var protoTile = c.Tile.ProtoTile;
                      if (protoTile
                          is TileBarren
                          or TileSwamp)
                      {
                          return true;
                      }

                      // unsuitable ground type
                      return false;
                  });

        private static readonly ConstructionTileRequirements.Validator ValidatorTooCloseToAnotherOilPump
            = new(ErrorCodePump.TooCloseToAnotherOilPump,
                  c =>
                  {
                      if (PveSystem.SharedIsPve(false))
                      {
                          // no distance limit in PvE
                          return true;
                      }

                      var startPosition = c.StartTilePosition;
                      var objectsInBounds = SharedFindObjectsNearby<IProtoObjectStructure>(startPosition);
                      foreach (var obj in objectsInBounds)
                      {
                          if (ReferenceEquals(obj, c.ObjectToRelocate))
                          {
                              continue;
                          }

                          switch (obj.ProtoStaticWorldObject)
                          {
                              case ProtoObjectOilPump:
                                  // found another extractor nearby
                                  return false;

                              case ProtoObjectConstructionSite
                                  when ProtoObjectConstructionSite.SharedGetConstructionProto(obj) is
                                           ProtoObjectOilPump:
                                  // found a blueprint for another extractor nearby
                                  return false;
                          }
                      }

                      return true;
                  });

        private static readonly ConstructionTileRequirements.Validator ValidatorTooCloseToDeposit
            = new(ErrorCode.CannotBuildTooCloseToDeposit,
                  c =>
                  {
                      var startPosition = c.StartTilePosition;
                      var objectsInBounds = SharedFindObjectsNearby<ObjectDepositOilSeep>(startPosition);
                      foreach (var obj in objectsInBounds)
                      {
                          if (startPosition == obj.TilePosition)
                          {
                              // can build right over the source
                              continue;
                          }

                          // found a deposit nearby but not right under it - cannot build too close to a deposit
                          return false;
                      }

                      return true;
                  });

        [RemoteEnum]
        public enum ErrorCodePump : byte
        {
            [Description(ErrorTooCloseToAnotherOilPump)]
            TooCloseToAnotherOilPump
        }

        public override byte ContainerInputSlotsCount => 1;

        public override byte ContainerOutputSlotsCount => 1;

        public override ElectricityThresholdsPreset DefaultConsumerElectricityThresholds
            => new(startupPercent: 30,
                   shutdownPercent: 20);

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
            tileRequirements
                .Clear()
                .Add(ValidatorGroundTypeOrOilSeep)
                .Add(ValidatorTooCloseToAnotherOilPump)
                .Add(ValidatorTooCloseToDeposit)
                .Add(ValidatorTooCloseToDepletedDeposit)
                .Add(ConstructionTileRequirements.BasicRequirements)
                .Add(ConstructionTileRequirements.ValidatorClientOnlyNoCurrentPlayer)
                .Add(ConstructionTileRequirements.ValidatorNoPhysicsBodyDynamic)
                .Add(ConstructionTileRequirements.ErrorCode.NoFreeSpace,
                     c => !ConstructionTileRequirements.TileHasAnyPhysicsObjectsWhere(
                              c.Tile,
                              o => o.PhysicsBody.IsStatic
                                   && o.PhysicsBody.AssociatedWorldObject?.ProtoWorldObject
                                       is not ObjectDepositOilSeep))
                .Add(ConstructionTileRequirements.ErrorCode.NoFreeSpace,
                     c => c.Tile.StaticObjects.All(
                         o => o.ProtoWorldObject is ObjectDepositOilSeep
                              || o.ProtoStaticWorldObject.Kind == StaticObjectKind.Floor
                              || o.ProtoStaticWorldObject.Kind == StaticObjectKind.FloorDecal))
                .Add(ConstructionTileRequirements.ValidatorNotRestrictedArea)
                .Add(LandClaimSystem.ValidatorIsOwnedLand);

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
            foreach (var o in tile.StaticObjects)
            {
                if (o.ProtoGameObject is ObjectDepositOilSeep)
                {
                    return o;
                }
            }

            return null;
        }
    }
}