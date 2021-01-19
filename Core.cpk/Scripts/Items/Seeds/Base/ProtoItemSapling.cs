namespace AtomicTorch.CBND.CoreMod.Items.Seeds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.State;

    public abstract class ProtoItemSapling
        <TPrivateState, TPublicState, TClientState>
        : ProtoItemSeed<TPrivateState, TPublicState, TClientState>, IProtoItemSapling
        where TPrivateState : BasePrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public const string ErrorSoilNotSuitable = "This tree type can't grow in this soil.";

        public const string ErrorTooCloseToOtherTrees =
            "To plant this sapling, there must be at least one tile of space between it and other trees.";

        public override double GroundIconScale => 2;

        public override ushort MaxItemsPerStack => ItemStackSize.Small;

        protected abstract void PrepareProtoItemSapling(
            out IProtoObjectVegetation objectPlantProto);

        protected sealed override void PrepareProtoItemSeed(
            out IProtoObjectVegetation objectPlantProto,
            List<IProtoObjectFarm> allowedToPlaceAt)
        {
            this.PrepareProtoItemSapling(out objectPlantProto);
        }

        protected override ConstructionTileRequirements PrepareTileRequirements()
        {
            if (this.AllowedToPlaceAtFarmObjects.Count > 0)
            {
                throw new Exception("Should not have allowed farm object types");
            }

            // placement conditions very similar to ProtoObjectFarmPlot
            return new ConstructionTileRequirements()
                   .Add(ConstructionTileRequirements.DefaultForStaticObjects)
                   .Add(ErrorSoilNotSuitable, c => c.Tile.ProtoTile is IProtoTileFarmAllowed)
                   .Add(ConstructionTileRequirements.ValidatorNoFloor)
                   .Add(ConstructionTileRequirements.ValidatorNotRestrictedArea)
                   .Add(LandClaimSystem.ValidatorIsOwnedOrFreeLandNoFactionPermissionsRequired)
                   .Add(ErrorTooCloseToOtherTrees,
                        context => context.Tile
                                          .EightNeighborTiles
                                          .All(t => !t.StaticObjects
                                                      .Any(so => so.ProtoStaticWorldObject is IProtoObjectTree)));
        }
    }

    public abstract class ProtoItemSapling
        : ProtoItemSapling
            <EmptyPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}