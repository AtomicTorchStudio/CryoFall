namespace AtomicTorch.CBND.CoreMod.Zones
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Props.Road;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Data.Zones;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SpawnMobsBroodNest : ProtoZoneSpawnScript
    {
        private readonly double spawnRateMultiplier = 1.0;

        // because this script called very rarely we're increasing the spawn attempts count
        protected override double MaxSpawnAttemptsMultiplier => 150;

        protected override void PrepareZoneSpawnScript(Triggers triggers, SpawnList spawnList)
        {
            // this resource is not spawned on the world init
            triggers
                // trigger on time interval
                .Add(GetTrigger<TriggerTimeInterval>().ConfigureForSpawn(TimeSpan.FromMinutes(30)));

            var presetBroodNest = spawnList.CreatePreset(interval: 130, padding: 4, useSectorDensity: false)
                                           .Add<MobBroodNest>()
                                           .SetCustomPaddingWithSelf(69);

            // don't spawn close to roads
            var restrictionPresetRoads = spawnList.CreateRestrictedPreset()
                                                  .Add<ObjectPropRoadHorizontal>()
                                                  .Add<ObjectPropRoadVertical>();
            presetBroodNest.SetCustomPaddingWith(restrictionPresetRoads, 30);
        }

        protected override IGameObjectWithProto ServerSpawnStaticObject(
            IProtoTrigger trigger,
            IServerZone zone,
            IProtoStaticWorldObject protoStaticWorldObject,
            Vector2Ushort tilePosition)
        {
            // ensure there are no cliff neighbor tiles
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
                        return null;
                    }
                }
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

            // respawn no more than 66% of brood nests per iteration
            // but scale automatically with the number of players online
            var spawnMaxPerIteration = (int)Math.Ceiling(desiredCountByDensity
                                                         * 0.66
                                                         * ServerSpawnRateScaleHelper.CalculateCurrentRate());

            return Math.Min(currentCount + spawnMaxPerIteration,
                            desiredCountByDensity);
        }
    }
}