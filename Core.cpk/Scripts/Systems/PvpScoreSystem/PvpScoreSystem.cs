namespace AtomicTorch.CBND.CoreMod.Systems.PvpScoreSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Helpers;

    [PrepareOrder(afterType: typeof(ServerTimersSystem))]
    public class PvpScoreSystem : ProtoSystem<PvpScoreSystem>
    {
        /// <summary>
        /// The server will collect damage sources leading to death for the last 30 seconds only.
        /// </summary>
        private const int DamageSourcesHistoryDuration = 30;

        /// <summary>
        /// The PvP kill is counted only if the total amount of PvP damage is above 10%.
        /// </summary>
        private const double MinTotalPvpDamageFractionToCountPvpKill = 0.1;

        /// <summary>
        /// Players are loosing 20% of their PvP score on death (but not less than 1 point, if has at least 1 point).
        /// </summary>
        private const double PvpScoreLossOnDeathFraction = 0.2; // ensure the value is in range from >0 to <1

        private const double PvpScoreRestoreDuration = 24 * 60; // 24 hours

        public static string ServerGetPvpLeaderboardReport(bool sortByKillDeathRatio, int minKills)
        {
            var sorted = sortByKillDeathRatio
                             ? ServerGetPvpLeaderboardByKillDeathRatio(minKills)
                             : ServerGetPvpLeaderboard();
            var sb = new StringBuilder();

            if (sortByKillDeathRatio)
            {
                sb.AppendLine("PvP leaderboard, top-100, sorted by KD with min " + minKills + " kills:")
                  .Append("(format: K/D | kills | deaths | total LP | PvP score)");
            }
            else
            {
                sb.AppendLine("PvP leaderboard, top-100:")
                  .Append("(format: PvP score | K/D | kills | deaths | total LP)");
            }

            if (sorted.Count == 0)
            {
                sb.AppendLine()
                  .Append("<the leaderboard is empty>");
                return sb.ToString();
            }

            for (var index = 0; index < sorted.Count; index++)
            {
                var entry = sorted[index];
                sb.AppendLine()
                  .Append("#")
                  .Append(index + 1)
                  .Append(" ");

                var clanTag = FactionSystem.SharedGetClanTag(entry.Character);
                if (!string.IsNullOrEmpty(clanTag))
                {
                    sb.Append("[")
                      .Append(clanTag)
                      .Append("] ");
                }

                var characterPrivateState = PlayerCharacter.GetPrivateState(entry.Character);
                var statistics = characterPrivateState.Statistics;
                if (sortByKillDeathRatio)
                {
                    sb.Append(entry.Character.Name)
                      .Append(" - KD: ")
                      .Append(statistics.PvpKillDeathRatio.ToString("0.00"))
                      .Append(", K: ")
                      .Append(statistics.PvpKills)
                      .Append(", D: ")
                      .Append(statistics.Deaths)
                      .Append(", LP: ")
                      .Append(characterPrivateState.Technologies.LearningPointsAccumulatedTotal)
                      .Append(", Score: ")
                      .Append((uint)entry.Score)
                      .Append(" points");
                }
                else
                {
                    sb.Append(entry.Character.Name)
                      .Append(" - ")
                      .Append((uint)entry.Score)
                      .Append(" points, KD: ")
                      .Append(statistics.PvpKillDeathRatio.ToString("0.00"))
                      .Append(", K: ")
                      .Append(statistics.PvpKills)
                      .Append(", D: ")
                      .Append(statistics.Deaths)
                      .Append(", LP: ")
                      .Append(characterPrivateState.Technologies.LearningPointsAccumulatedTotal);
                }
            }

            return sb.ToString();
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            ServerCharacterDeathMechanic.CharacterDeath += ServerCharacterDeathHandler;

            if (PveSystem.ServerIsPvE)
            {
                return;
            }

            TriggerEveryFrame.ServerRegister(
                callback: ServerRecoverPvpScore,
                name: "System." + this.ShortId);

            // print PvP leaderboard after the server initialization is over
            // (it will also enable printing it every hour)
            ServerTimersSystem.AddAction(
                delaySeconds: 1,
                ServerPrintPvpLeaderboardEveryHour);
        }

        private static void IncreasePvpKillsCounter(ICharacter killer, ICharacter deadCharacter)
        {
            var statistics = PlayerCharacter.GetPrivateState(killer).Statistics;
            statistics.PvpKills++;
            Logger.Important(
                string.Format("{0} has killed {1}, increasing kills counter. Total kills: {2}",
                              killer,
                              deadCharacter,
                              statistics.PvpKills));
        }

        private static void ServerCharacterDeathHandler(ICharacter deadCharacter)
        {
            if (deadCharacter.IsNpc)
            {
                return;
            }

            var statistics = PlayerCharacter.GetPrivateState(deadCharacter).Statistics;
            statistics.Deaths++;
            Logger.Important($"{deadCharacter} died. Total deaths: {statistics.Deaths}");

            if (PveSystem.ServerIsPvE)
            {
                return;
            }

            using var tempKilledBy = ServerGetKilledByPlayerCharacters(deadCharacter);
            if (tempKilledBy is null
                || tempKilledBy.Count == 0)
            {
                return;
            }

            // increase kills counter only for the first killer (with the highest damage ratio)
            IncreasePvpKillsCounter(killer: tempKilledBy.AsList()[0].Killer,
                                    deadCharacter);

            if (NewbieProtectionSystem.SharedIsNewbie(deadCharacter))
            {
                Logger.Important(
                    $"{deadCharacter} was killed while under newbie protection, PvP score mechanic doesn't apply");
                return;
            }

            var originalPvpScore = statistics.PvpScore;
            if (originalPvpScore <= 0)
            {
                // cannot take PvP score
                Logger.Important($"{deadCharacter} was killed but has PvP score 0 so no score is lost");
                return;
            }

            double pvpScoreTaken;

            if (originalPvpScore <= 1)
            {
                pvpScoreTaken = 1;
            }
            else
            {
                pvpScoreTaken = MathHelper.Clamp(Math.Floor(originalPvpScore * PvpScoreLossOnDeathFraction),
                                                 min: 1,
                                                 max: originalPvpScore);
            }

            statistics.PvpScore = Math.Max(0, originalPvpScore - pvpScoreTaken);
            if (statistics.PvpScore == 0)
            {
                statistics.ServerPvpScoreNextRecoveryTime = Server.Game.FrameTime + PvpScoreRestoreDuration;
            }

            Logger.Important(
                string.Format(
                    "{0} was killed in PvP and lost PvP score: {1}Previous score: {2:0.##}{1}New score: {3:0.##}",
                    deadCharacter,
                    Environment.NewLine,
                    originalPvpScore,
                    statistics.PvpScore));

            ServerDistributePvpScore(tempKilledBy.AsList(), pvpScoreTaken, deadCharacter);
        }

        private static void ServerDistributePvpScore(
            List<(ICharacter Killer, double Fraction)> killedBy,
            double pvpScoreTaken,
            ICharacter deadCharacter)
        {
            if (pvpScoreTaken < 1)
            {
                throw new Exception("Incorrect amount of PvP score taken, it cannot be below 1");
            }

            // enumerate all killers (the list is ordered by the damage fraction)
            var remainingPvpScoreTaken = pvpScoreTaken;
            foreach (var entry in killedBy)
            {
                var killerScoreShare = entry.Fraction * remainingPvpScoreTaken;
                if (killerScoreShare < 1)
                {
                    // take the remainder (if there are other participants, they will not receive any rewards)
                    killerScoreShare = remainingPvpScoreTaken;
                }

                var killerStatistics = PlayerCharacter.GetPrivateState(entry.Killer)
                                                      .Statistics;
                killerStatistics.PvpScore += killerScoreShare;
                killerStatistics.ServerPvpScoreNextRecoveryTime = 0;

                Logger.Important(
                    string.Format("{0} has killed {1}, increasing PvP score +{2:0.##}. Total PvP score {3:0.##}",
                                  entry.Killer,
                                  deadCharacter,
                                  killerScoreShare,
                                  killerStatistics.PvpScore));

                remainingPvpScoreTaken -= killerScoreShare;
                if (remainingPvpScoreTaken <= 0)
                {
                    break;
                }
            }
        }

        private static ITempList<(ICharacter Killer, double Fraction)> ServerGetKilledByPlayerCharacters(
            ICharacter deadCharacter)
        {
            var privateState = PlayerCharacter.GetPrivateState(deadCharacter);
            var damageSources = CharacterDamageTrackingSystem.ServerGetDamageSourcesAfterTime(
                deadCharacter,
                // ReSharper disable once PossibleInvalidOperationException
                afterTime: privateState.LastDeathTime.Value - DamageSourcesHistoryDuration);

            if (damageSources is null
                || damageSources.Count == 0)
            {
                Logger.Important("No recent damage sources obtained for character death: " + deadCharacter);
                return null;
            }

            var totalPvpFraction = 0.0;
            var tempKilledBy = Api.Shared.GetTempList<(ICharacter Killer, double Fraction)>();
            foreach (var damageSource in damageSources)
            {
                if (damageSource.ProtoEntity is not PlayerCharacter)
                {
                    continue;
                }

                var killer = Server.Characters.GetPlayerCharacter(damageSource.Name);
                if (killer is null
                    || FactionSystem.SharedArePlayersInTheSameFaction(killer, deadCharacter)
                    || PartySystem.ServerIsSameParty(killer, deadCharacter))
                {
                    continue;
                }

                if (NewbieProtectionSystem.SharedIsNewbie(killer))
                {
                    // don't provide PvP score to killers under newbie protection
                    // (also, it should be impossible to kill anyone while under newbie protection)
                    continue;
                }

                tempKilledBy.Add((killer, damageSource.Fraction));
                totalPvpFraction += damageSource.Fraction;
            }

            if (totalPvpFraction <= 0)
            {
                // didn't die from any player's damage
                Logger.Important($"{deadCharacter} didn't die from any PvP damage. PvP score is not affected");
                return null;
            }

            if (totalPvpFraction < MinTotalPvpDamageFractionToCountPvpKill)
            {
                // didn't die from any player's damage
                Logger.Important(
                    $"{deadCharacter} didn't die from any significant PvP damage (only {(totalPvpFraction * 100):0.##}% of recent damage is from PvP). PvP score is not affected");
                return null;
            }

            // normalize participation fraction
            var list = tempKilledBy.AsList();
            for (var index = 0; index < list.Count; index++)
            {
                var entry = list[index];
                list[index] = (entry.Killer, entry.Fraction / totalPvpFraction);
            }

            return tempKilledBy;
        }

        private static List<(ICharacter Character, double Score)> ServerGetPvpLeaderboard()
        {
            return Api.Server.Characters
                      .EnumerateAllPlayerCharacters(onlyOnline: false,
                                                    exceptSpectators: false)
                      .Select(c => (Character: c,
                                    Score: PlayerCharacter.GetPrivateState(c).Statistics.PvpScore))
                      .Where(c => c.Score > 1)
                      .OrderByDescending(c => c.Score)
                      .Take(100)
                      .ToList();
        }

        private static List<(ICharacter Character, double Score)> ServerGetPvpLeaderboardByKillDeathRatio(int minKills)
        {
            return Api.Server.Characters
                      .EnumerateAllPlayerCharacters(onlyOnline: false,
                                                    exceptSpectators: false)
                      .Where(c => PlayerCharacter.GetPrivateState(c).Statistics.PvpKills >= minKills)
                      .Select(c => (Character: c,
                                    Score: PlayerCharacter.GetPrivateState(c).Statistics.PvpScore))
                      .OrderByDescending(c => PlayerCharacter.GetPrivateState(c.Character).Statistics.PvpKillDeathRatio)
                      .Take(100)
                      .ToList();
        }

        private static void ServerPrintPvpLeaderboardEveryHour()
        {
            try
            {
                Logger.Important(ServerGetPvpLeaderboardReport(sortByKillDeathRatio: false, minKills: 0));
                Logger.Important(ServerGetPvpLeaderboardReport(sortByKillDeathRatio: true,  minKills: 10));
            }
            finally
            {
                ServerTimersSystem.AddAction(
                    delaySeconds: 60 * 60,
                    ServerPrintPvpLeaderboardEveryHour);
            }
        }

        /// <summary>
        /// This method will recover 1 PvP score for players that have lost their PvP score completely
        /// and didn't recovered it by killing anyone.
        /// The recovery doesn't happen instantly (see PvpScoreRestoreDuration).
        /// </summary>
        private static void ServerRecoverPvpScore()
        {
            // perform update once per 20 seconds per player
            const double spreadDeltaTime = 20;

            using var tempListPlayers = Api.Shared.GetTempList<ICharacter>();
            PlayerCharacter.Instance
                           .EnumerateGameObjectsWithSpread(tempListPlayers.AsList(),
                                                           spreadDeltaTime: spreadDeltaTime,
                                                           Server.Game.FrameNumber,
                                                           Server.Game.FrameRate);

            var serverTime = Server.Game.FrameTime;
            foreach (var character in tempListPlayers.AsList())
            {
                var statistics = PlayerCharacter.GetPrivateState(character).Statistics;
                if (statistics.ServerPvpScoreNextRecoveryTime <= 0)
                {
                    if (statistics.PvpScore == 0)
                    {
                        // (the server has rebooted?) Restore PvP score instantly 
                        statistics.PvpScore = 1;
                        Logger.Important("PvP score was 0. Restored PvP score to 1 for: " + character);
                    }

                    continue;
                }

                if (serverTime < statistics.ServerPvpScoreNextRecoveryTime)
                {
                    continue;
                }

                // restore PvP score for this player
                if (statistics.PvpScore <= 0)
                {
                    statistics.PvpScore = 1;
                }

                statistics.ServerPvpScoreNextRecoveryTime = 0;
                Logger.Important("Restored PvP score to 1 for: " + character);
            }
        }
    }
}