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
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Extensions;

    public abstract class ProtoObjectLithiumOreExtractor : ProtoObjectExtractor
    {
        public const string ErrorTooCloseToAnotherExtractor
            = "Too close to another extractor.";

        private static readonly ConstructionTileRequirements.Validator ValidatorGroundTypeOrGeothermalSpring
            = new(() => string.Format("[b]{0}[/b][br]{1}[*]{2}[*]{3}[*]{4}[*]{5}",
                                      ConstructionTileRequirements.ErrorCode.UnsuitableGround_Title
                                                                  .GetDescription(),
                                      ConstructionTileRequirements.ErrorCode.UnsuitableGround_Message_CanBuildOnlyOn
                                                                  .GetDescription(),
                                      Api.GetProtoEntity<TileForestTropical>().Name,
                                      Api.GetProtoEntity<TileForestTemperate>().Name,
                                      Api.GetProtoEntity<TileForestBoreal>().Name,
                                      Api.GetProtoEntity<TileMeadows>().Name),
                  c =>
                  {
                      if (PveSystem.SharedIsPve(true))
                      {
                          // no soil requirement in PvE
                          return true;
                      }

                      foreach (var obj in c.Tile.StaticObjects)
                      {
                          if (obj.ProtoStaticWorldObject is ObjectDepositGeothermalSpring)
                          {
                              return true;
                          }
                      }

                      var protoTile = c.Tile.ProtoTile;
                      return protoTile
                                 is TileForestTropical
                                 or TileForestTemperate
                                 or TileForestBoreal
                                 or TileMeadows;
                  });

        private static readonly ConstructionTileRequirements.Validator ValidatorTooCloseToAnotherExtractor
            = new(ErrorCodeExtractor.TooCloseToAnotherExtractor,
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
                              case ProtoObjectLithiumOreExtractor:
                                  // found another extractor nearby
                                  return false;

                              case ProtoObjectConstructionSite
                                  when ProtoObjectConstructionSite.SharedGetConstructionProto(obj) is
                                           ProtoObjectLithiumOreExtractor:
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
                      var objectsInBounds = SharedFindObjectsNearby<ObjectDepositGeothermalSpring>(startPosition);
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
        public enum ErrorCodeExtractor : byte
        {
            [Description(ErrorTooCloseToAnotherExtractor)]
            TooCloseToAnotherExtractor
        }

        public override byte ContainerInputSlotsCount => 0;

        public override byte ContainerOutputSlotsCount => 1;

        public override ElectricityThresholdsPreset DefaultConsumerElectricityThresholds
            => new(startupPercent: 30,
                   shutdownPercent: 20);

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

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 1.8;
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            tileRequirements
                .Clear()
                .Add(ValidatorGroundTypeOrGeothermalSpring)
                .Add(ValidatorTooCloseToAnotherExtractor)
                .Add(ValidatorTooCloseToDeposit)
                .Add(ValidatorTooCloseToDepletedDeposit)
                .Add(ConstructionTileRequirements.BasicRequirements)
                .Add(ConstructionTileRequirements.ValidatorClientOnlyNoCurrentPlayer)
                .Add(ConstructionTileRequirements.ValidatorNoPhysicsBodyDynamic)
                .Add(ConstructionTileRequirements.ErrorCode.NoFreeSpace,
                     c => !ConstructionTileRequirements.TileHasAnyPhysicsObjectsWhere(
                              c.Tile,
                              t => t.PhysicsBody.IsStatic
                                   && t.PhysicsBody.AssociatedWorldObject?.ProtoWorldObject
                                       is not ObjectDepositGeothermalSpring))
                .Add(ConstructionTileRequirements.ErrorCode.NoFreeSpace,
                     c => c.Tile.StaticObjects.All(
                         o => o.ProtoWorldObject is ObjectDepositGeothermalSpring
                              || o.ProtoStaticWorldObject.Kind == StaticObjectKind.Floor
                              || o.ProtoStaticWorldObject.Kind == StaticObjectKind.FloorDecal))
                .Add(ConstructionTileRequirements.ValidatorNotRestrictedArea)
                .Add(LandClaimSystem.ValidatorIsOwnedLand);

            this.PrepareConstructionConfig(build, repair, out category);
        }

        protected abstract void PrepareConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            out ProtoStructureCategory category);

        protected override IStaticWorldObject SharedGetDepositWorldObject(Tile tile)
        {
            foreach (var o in tile.StaticObjects)
            {
                if (o.ProtoGameObject is ObjectDepositGeothermalSpring)
                {
                    return o;
                }
            }

            return null;
        }
    }
}