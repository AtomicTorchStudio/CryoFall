namespace AtomicTorch.CBND.CoreMod.Helpers.Server
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ServerSearchAreaHelper
    {
        private static readonly IWorldServerService World = Api.Server.World;

        /// <summary>
        /// Generates the search circle area position and radius for the provided position.
        /// The search area will contain at least about 33% of the provided biome tiles
        /// and will include the provided world position.
        /// </summary>
        /// <returns>False if cannot generate a search area.</returns>
        public static bool GenerateSearchArea(
            Vector2Ushort worldPosition,
            IProtoTile biome,
            ushort circleRadius,
            out Vector2Ushort circleCenter,
            int maxAttempts,
            double waterMaxRatio = 0.3)
        {
            var biomeSessionIndex = biome.SessionIndex;

            if (TryToCreateSearchArea(desiredBiomeMatchRatio: 0.75,    out circleCenter)
                || TryToCreateSearchArea(desiredBiomeMatchRatio: 0.5,  out circleCenter)
                || TryToCreateSearchArea(desiredBiomeMatchRatio: 0.33, out circleCenter))
            {
                return true;
            }

            return false;

            bool TryToCreateSearchArea(
                double desiredBiomeMatchRatio,
                out Vector2Ushort circleCenter)
            {
                for (var attempt = 0; attempt < maxAttempts; attempt++)
                {
                    var offset = circleRadius * RandomHelper.NextDouble();
                    var angle = RandomHelper.NextDouble() * MathConstants.DoublePI;
                    var resultD = new Vector2D(worldPosition.X + offset * Math.Cos(angle),
                                               worldPosition.Y + offset * Math.Sin(angle));

                    circleCenter = new Vector2Ushort((ushort)MathHelper.Clamp(resultD.X, 0, ushort.MaxValue),
                                                     (ushort)MathHelper.Clamp(resultD.Y, 0, ushort.MaxValue));

                    if (IsValidCircle(circleCenter))
                    {
                        return true;
                    }

                    bool IsValidCircle(Vector2Ushort circleCenter)
                    {
                        uint totalChecks = 0,
                             biomeMathes = 0,
                             waterOrOutOfBounds = 0;
                        for (var x = -circleRadius; x < circleRadius; x += 10)
                        for (var y = -circleRadius; y < circleRadius; y += 10)
                        {
                            totalChecks++;
                            var tile = World.GetTile(circleCenter.X + x,
                                                     circleCenter.Y + y,
                                                     logOutOfBounds: false);
                            if (tile.IsOutOfBounds)
                            {
                                waterOrOutOfBounds++;
                                biomeMathes++; // yes, consider it a biome match
                                continue;
                            }

                            if (tile.ProtoTileSessionIndex == biomeSessionIndex)
                            {
                                biomeMathes++;
                            }
                            else if (tile.ProtoTile.Kind == TileKind.Water)
                            {
                                waterOrOutOfBounds++;
                                biomeMathes++; // yes, consider it a biome match
                            }
                        }

                        var biomeMatchRatio = biomeMathes / (double)totalChecks;
                        var waterOrOutOfBoundsRatio = waterOrOutOfBounds / (double)totalChecks;
                        return biomeMatchRatio >= desiredBiomeMatchRatio
                               && waterOrOutOfBoundsRatio <= waterMaxRatio;
                    }
                }

                circleCenter = default;
                return false;
            }
        }
    }
}