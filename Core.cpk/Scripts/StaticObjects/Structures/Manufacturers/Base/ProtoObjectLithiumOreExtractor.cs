namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
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
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class ProtoObjectLithiumOreExtractor : ProtoObjectExtractor
    {
        public const string ErrorTooCloseToAnotherExtractor
            = "Too close to another extractor.";

        private static readonly ConstructionTileRequirements.Validator ValidatorGroundTypeOrGeothermalSpring
            = new ConstructionTileRequirements.Validator(
                () => string.Format("[b]{0}[/b][br]{1}[*]{2}[*]{3}[*]{4}[*]{5}",
                                    ConstructionTileRequirements.Error_UnsuitableGround_Title,
                                    ConstructionTileRequirements.Error_UnsuitableGround_Message_CanBuildOnlyOn,
                                    Api.GetProtoEntity<TileForestTropical>().Name,
                                    Api.GetProtoEntity<TileForestTemperate>().Name,
                                    Api.GetProtoEntity<TileForestBoreal>().Name,
                                    Api.GetProtoEntity<TileMeadows>().Name),
                c =>
                {
                    foreach (var obj in c.Tile.StaticObjects)
                    {
                        if (obj.ProtoStaticWorldObject is ObjectDepositGeothermalSpring)
                        {
                            return true;
                        }
                    }

                    var protoTile = c.Tile.ProtoTile;
                    return protoTile is TileForestTropical
                           || protoTile is TileForestTemperate
                           || protoTile is TileForestBoreal
                           || protoTile is TileMeadows;
                });

        private static readonly ConstructionTileRequirements.Validator ValidatorTooCloseToAnotherExtractor
            = new ConstructionTileRequirements.Validator(
                ErrorTooCloseToAnotherExtractor,
                c =>
                {
                    if (c.TileOffset != default)
                    {
                        return true;
                    }

                    if (PveSystem.SharedIsPve(false))
                    {
                        // no distance limit in PvE
                        return true;
                    }

                    var startPosition = c.Tile.Position;
                    var objectsInBounds = SharedFindObjectsNearby<IProtoObjectStructure>(startPosition);
                    foreach (var obj in objectsInBounds)
                    {
                        switch (obj.ProtoStaticWorldObject)
                        {
                            case ProtoObjectLithiumOreExtractor _:
                                // found another extractor nearby
                                return false;

                            case ProtoObjectConstructionSite _
                                when ProtoObjectConstructionSite.SharedGetConstructionProto(obj) is
                                         ProtoObjectLithiumOreExtractor:
                                // found a blueprint for another extractor nearby
                                return false;
                        }
                    }

                    return true;
                });

        private static readonly ConstructionTileRequirements.Validator ValidatorTooCloseToDeposit
            = new ConstructionTileRequirements.Validator(
                Error_CannotBuildTooCloseToDeposit,
                c =>
                {
                    if (c.TileOffset != default)
                    {
                        return true;
                    }

                    var startPosition = c.Tile.Position;
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

        public override byte ContainerInputSlotsCount => 0;

        public override byte ContainerOutputSlotsCount => 1;

        public override ElectricityThresholdsPreset DefaultConsumerElectricityThresholds
            => new ElectricityThresholdsPreset(startupPercent: 30,
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
                .Add(ConstructionTileRequirements.ErrorNoFreeSpace,
                     c => !ConstructionTileRequirements.TileHasAnyPhysicsObjectsWhere(
                              c.Tile,
                              t => t.PhysicsBody.IsStatic
                                   && !(t.PhysicsBody.AssociatedWorldObject?.ProtoWorldObject is
                                            ObjectDepositGeothermalSpring)))
                .Add(ConstructionTileRequirements.ErrorNoFreeSpace,
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
            return tile.StaticObjects.FirstOrDefault(
                o => o.ProtoStaticWorldObject is ObjectDepositGeothermalSpring);
        }
    }
}