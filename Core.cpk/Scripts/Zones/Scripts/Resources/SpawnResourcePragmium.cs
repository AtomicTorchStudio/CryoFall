namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Props.Road;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SpawnResourcePragmium : ProtoZoneSpawnScript
    {
        public const int PaddingPragmiumWithOilDeposit
            = ObjectMineralPragmiumSourceExplosion.ExplosionDamageRadius + 6;

        private double spawnRateMultiplier;

        // because this script called very rare we're increasing the spawn attempts count
        protected override double MaxSpawnAttemptsMultiplier => 150;

        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            this.spawnRateMultiplier = RateResourcesPragmiumSourceMultiplier.SharedValue;

            // this resource is not spawned on the world init
            triggers
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(30)));

            var presetPragmiumSource = spawnList.CreatePreset(interval: 183, padding: 2, useSectorDensity: false)
                                                .Add<ObjectMineralPragmiumSource>()
                                                .SetCustomPaddingWithSelf(79);

            // don't spawn close to oil seeps
            var restrictionPresetDepositOilSeep = spawnList.CreateRestrictedPreset()
                                                           .Add<ObjectDepositOilSeep>();
            presetPragmiumSource.SetCustomPaddingWith(restrictionPresetDepositOilSeep,
                                                      PaddingPragmiumWithOilDeposit);

            // don't spawn close to roads
            var restrictionPresetRoads = spawnList.CreateRestrictedPreset()
                                                  .Add<ObjectPropRoadHorizontal>()
                                                  .Add<ObjectPropRoadVertical>();
            presetPragmiumSource.SetCustomPaddingWith(restrictionPresetRoads, 30);

            // special restriction preset for player land claims
            var restrictionPresetLandclaim = spawnList.CreateRestrictedPreset()
                                                      .Add<IProtoObjectLandClaim>();

            // Let's ensure that we don't spawn Pragmium Source too close to players' buildings.
            // take half size of the largest land claim area
            var paddingToLandClaimsSize = LandClaimSystem.MaxLandClaimSizeWithGraceArea.Value / 2.0;
            // add the explosion radius
            paddingToLandClaimsSize += Api.GetProtoEntity<ObjectMineralPragmiumSourceExplosion>()
                                          .DamageRadius;
            // add few extra tiles (as the objects are not 1*1 tile)
            paddingToLandClaimsSize += 6;

            presetPragmiumSource.SetCustomPaddingWith(restrictionPresetLandclaim, paddingToLandClaimsSize);
        }

        protected override IGameObjectWithProto ServerSpawnStaticObject(
            IProtoTrigger trigger,
            IServerZone zone,
            IProtoStaticWorldObject protoStaticWorldObject,
            Vector2Ushort tilePosition)
        {
            // ensure there is at least a single cliff neighbor tile
            var hasCliffNeighborTile = false;
            foreach (var tileOffset in protoStaticWorldObject.Layout.TileOffsets)
            {
                if (tileOffset.X == 0
                    && tileOffset.Y == 0)
                {
                    continue;
                }

                var tile = Server.World.GetTile(tilePosition.X + 2 * tileOffset.X,
                                                tilePosition.Y + 2 * tileOffset.Y);
                foreach (var neighborTile in tile.EightNeighborTiles)
                {
                    if (neighborTile.IsCliffOrSlope)
                    {
                        hasCliffNeighborTile = true;
                        break;
                    }
                }

                if (hasCliffNeighborTile)
                {
                    break;
                }
            }

            if (!hasCliffNeighborTile)
            {
                return null;
            }

            return base.ServerSpawnStaticObject(trigger, zone, protoStaticWorldObject, tilePosition);
        }

        protected override int SharedCalculatePresetDesiredCount(
            ObjectSpawnPreset preset,
            IServerZone zone,
            int currentCount,
            int desiredCountByDensity)
        {
            desiredCountByDensity = (int)Math.Round(desiredCountByDensity * this.spawnRateMultiplier,
                                                    MidpointRounding.AwayFromZero);

            // respawn no more than 66% of pragmium source objects per iteration
            // but scale automatically with the number of players online
            var spawnMaxPerIteration = (int)Math.Ceiling(desiredCountByDensity
                                                         * 0.66
                                                         * ServerSpawnRateScaleHelper.CalculateCurrentRate());

            return Math.Min(currentCount + spawnMaxPerIteration,
                            desiredCountByDensity);
        }
    }
}