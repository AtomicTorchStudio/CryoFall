namespace AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem.Metrics
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// This metric gathers all the structures that are providing Faction score points
    /// within the faction-owned land claims, and sums up their amount.
    /// </summary>
    public class FactionScoreFactionBasesWealth : ProtoFactionScoreMetric
    {
        private static readonly IWorldServerService ServerWorld = IsServer ? Server.World : null;

        public override string Description =>
            "Awarded for structures that have a faction score points value, located inside of the faction-owned bases (only the land claims transferred to the faction ownership are counted).";

        public override double FinalScoreCoefficient => 1 / FactionLeaderboardSystem.TotalScoreMultiplier;

        public override string Name => "Wealth score";

        public override double PowerCoefficient => 1.0;

        public override double ServerGetCurrentValue(ILogicObject faction)
        {
            var structures = new HashSet<IStaticWorldObject>();
            var clanTag = FactionSystem.SharedGetClanTag(faction);

            foreach (var area in LandClaimSystem.SharedEnumerateAllFactionAreas(clanTag))
            {
                var areaBounds = LandClaimSystem.SharedGetLandClaimAreaBounds(area, addGracePadding: false);
                ServerGatherValuableStructures(areaBounds, structures);
            }

            if (structures.Count == 0)
            {
                return 0;
            }

            var result = 0.0;
            foreach (var obj in structures)
            {
                result += ((IProtoObjectStructure)obj.ProtoGameObject).FactionWealthScorePoints;
            }

            return result;
        }

        public override void ServerInitialize()
        {
        }

        private static void ServerGatherValuableStructures(
            RectangleInt areaBounds,
            HashSet<IStaticWorldObject> result)
        {
            for (var x = areaBounds.X; x < areaBounds.X + areaBounds.Width; x++)
            for (var y = areaBounds.Y; y < areaBounds.Y + areaBounds.Height; y++)
            {
                if (x < 0
                    || y < 0
                    || x >= ushort.MaxValue
                    || y >= ushort.MaxValue)
                {
                    continue;
                }

                var staticObjects = ServerWorld.GetStaticObjects(new Vector2Ushort((ushort)x, (ushort)y));
                foreach (var worldObject in staticObjects)
                {
                    var prototype = worldObject.ProtoStaticWorldObject;
                    if (prototype is IProtoObjectStructure protoObjectStructure
                        && protoObjectStructure.FactionWealthScorePoints > 0)
                    {
                        result.Add(worldObject);
                    }
                }
            }
        }
    }
}