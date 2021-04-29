namespace AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem.Metrics
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Systems.BossLootSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// One boss loot pile gives one boss score point.
    /// </summary>
    public class FactionScoreMetricBossScore : ProtoFactionScoreMetric
    {
        public override string Description =>
            "Awarded for killing world bosses, proportional to the amount of the loot share received.";

        public override double FinalScoreCoefficient => 300;

        public override string Name => "Boss score";

        public override double PowerCoefficient => 0.9;

        public override double ServerGetCurrentValue(ILogicObject faction)
        {
            return Faction.GetPrivateState(faction).ServerMetricBossScore;
        }

        public override void ServerInitialize()
        {
            ServerBossLootSystem.BossDefeated += ServerBossDefeatedHandler;
        }

        private static void ServerBossDefeatedHandler(
            IProtoCharacterMob protoCharacterBoss,
            Vector2Ushort bossPosition,
            List<ServerBossLootSystem.WinnerEntry> winnerEntries)
        {
            var lootByFaction = winnerEntries.GroupBy(e => FactionSystem.SharedGetClanTag(e.Character))
                                             .Where(g => !string.IsNullOrEmpty(g.Key))
                                             .ToDictionary(g => g.Key, g => g.Sum(l => l.LootCount));

            foreach (var pair in lootByFaction)
            {
                var clanTag = pair.Key;
                var faction = FactionSystem.ServerGetFactionByClanTag(clanTag);
                if (faction is null)
                {
                    Api.Logger.Error("Should be impossible - no faction for clan tag: " + clanTag);
                    continue;
                }

                var factionPrivateState = Faction.GetPrivateState(faction);
                factionPrivateState.ServerMetricBossScore += (ulong)pair.Value;
                /*Logger.Dev("Boss metric updated: " + factionPrivateState.ServerMetricBossScore);*/
            }
        }
    }
}