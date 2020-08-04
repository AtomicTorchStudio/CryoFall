namespace AtomicTorch.CBND.CoreMod.Characters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Events.Base;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ServerBossLootSystem : ProtoSystem<ServerBossLootSystem>
    {
        private static readonly ICharactersServerService ServerCharacters
            = IsServer ? Api.Server.Characters : null;

        private static readonly IWorldServerService ServerWorld
            = IsServer ? Api.Server.World : null;

        [NotLocalizable]
        public override string Name => "Boss loot system";

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public static void ServerCreateBossLoot(
            Vector2Ushort epicenterPosition,
            IProtoCharacterMob protoCharacterBoss,
            ServerBossDamageTracker damageTracker,
            double bossDifficultyCoef,
            IProtoStaticWorldObject lootObjectProto,
            int lootObjectsDefaultCount,
            double lootObjectsRadius,
            int maxLootWinners)
        {
            var approximatedTotalLootCountToSpawn = (int)Math.Ceiling(lootObjectsDefaultCount * bossDifficultyCoef);
            if (approximatedTotalLootCountToSpawn < 2)
            {
                approximatedTotalLootCountToSpawn = 2;
            }

            var allCharactersByDamage = damageTracker.GetDamageByCharacter();
            var winners = ServerSelectWinnersByParticipation(
                ServerSelectWinnersByDamage(allCharactersByDamage,
                                            maxLootWinners));

            var winnerEntries = winners.Select(
                                           c => new WinnerEntry(
                                               c.Character,
                                               damagePercent: (byte)Math.Max(1,
                                                                             Math.Round(c.Score * 100,
                                                                                        MidpointRounding
                                                                                            .AwayFromZero)),
                                               lootCount: CalculateLootCountForScore(c.Score)))
                                       .ToList();

            if (PveSystem.ServerIsPvE)
            {
                ServerNotifyCharactersNotEligibleForReward(
                    protoCharacterBoss,
                    allCharactersByDamage.Select(c => c.Character)
                                         .Except(winners.Select(c => c.Character))
                                         .ToList());

                ServerNotifyCharactersEligibleForReward(
                    protoCharacterBoss,
                    winnerEntries,
                    totalLootCount: (ushort)winnerEntries.Sum(e => (int)e.LootCount));
            }

            ServerSpawnLoot();
            
            // send victory announcement notification
            var winnerNamesWithClanTags = winnerEntries
                                          .Select(w => (Name: w.Character.Name,
                                                        ClanTag: PlayerCharacter.GetPublicState(w.Character).ClanTag))
                                          .ToArray();

            Instance.CallClient(
                ServerCharacters.EnumerateAllPlayerCharacters(onlyOnline: true),
                _ => _.ClientRemote_VictoryAnnouncement(protoCharacterBoss,
                                                        winnerNamesWithClanTags));

            Api.Logger.Important(
                protoCharacterBoss.ShortId
                + " boss defeated."
                + Environment.NewLine
                + "Damage by player:"
                + Environment.NewLine
                + allCharactersByDamage.Select(p => $" * {p.Character}: {p.Damage:F0}")
                                       .GetJoinedString(Environment.NewLine)
                + Environment.NewLine
                + "Player participation score: (only for selected winners)"
                + Environment.NewLine
                + winners.Select(p => $" * {p.Character}: {(p.Score * 100):F1}%")
                         .GetJoinedString(Environment.NewLine));

            byte CalculateLootCountForScore(double score)
            {
                var result = Math.Ceiling(approximatedTotalLootCountToSpawn * score);
                if (result < 1)
                {
                    return 1;
                }

                return (byte)Math.Min(byte.MaxValue, result);
            }

            void ServerSpawnLoot()
            {
                foreach (var winnerEntry in winnerEntries)
                {
                    if (!ServerTrySpawnLootForWinner(winnerEntry.Character, winnerEntry.LootCount))
                    {
                        // spawn attempts failure as logged inside the method,
                        // abort further spawning
                        return;
                    }
                }
            }

            bool ServerTrySpawnLootForWinner(ICharacter forCharacter, double countToSpawnRemains)
            {
                var attemptsRemains = 2000;

                while (countToSpawnRemains > 0)
                {
                    attemptsRemains--;
                    if (attemptsRemains <= 0)
                    {
                        // attempts exceeded
                        Api.Logger.Error(
                            "Cannot spawn all the loot for boss - number of attempts exceeded. Cont to spawn for winner remains: "
                            + countToSpawnRemains);
                        return false;
                    }

                    // calculate random distance from the explosion epicenter
                    var distance = RandomHelper.Range(2, lootObjectsRadius);

                    // ensure we spawn more objects closer to the epicenter
                    var spawnProbability = 1 - (distance / lootObjectsRadius);
                    spawnProbability = Math.Pow(spawnProbability, 1.25);
                    if (!RandomHelper.RollWithProbability(spawnProbability))
                    {
                        // random skip
                        continue;
                    }

                    var angle = RandomHelper.NextDouble() * MathConstants.DoublePI;
                    var spawnPosition = new Vector2Ushort(
                        (ushort)(epicenterPosition.X + distance * Math.Cos(angle)),
                        (ushort)(epicenterPosition.Y + distance * Math.Sin(angle)));

                    if (ServerTrySpawnLootObject(spawnPosition, forCharacter))
                    {
                        // spawned successfully!
                        countToSpawnRemains--;
                    }
                }

                return true;
            }

            bool ServerTrySpawnLootObject(Vector2Ushort spawnPosition, ICharacter forCharacter)
            {
                if (!lootObjectProto.CheckTileRequirements(spawnPosition,
                                                           character: null,
                                                           logErrors: false))
                {
                    return false;
                }

                var lootObject = ServerWorld.CreateStaticWorldObject(lootObjectProto, spawnPosition);
                if (lootObject is null)
                {
                    return false;
                }

                // mark the loot object for this player (works only in PvE)
                WorldObjectClaimSystem.ServerTryClaim(lootObject,
                                                      forCharacter,
                                                      WorldObjectClaimDuration.BossLoot,
                                                      claimForPartyMembers: false);
                return true;
            }
        }

        public static void ServerSpawnBossMinionsOnDeath(
            Vector2Ushort epicenterPosition,
            double bossDifficultyCoef,
            IProtoCharacter minionProto,
            int minionsDefaultCount,
            double minionsRadius)
        {
            var countToSpawnRemains = minionsDefaultCount;

            // apply difficulty coefficient
            countToSpawnRemains = (int)Math.Ceiling(countToSpawnRemains * bossDifficultyCoef);
            if (countToSpawnRemains < 2)
            {
                countToSpawnRemains = 2;
            }

            var attemptsRemains = 3000;

            while (countToSpawnRemains > 0)
            {
                attemptsRemains--;
                if (attemptsRemains <= 0)
                {
                    // attempts exceeded
                    return;
                }

                // calculate random distance from the explosion epicenter
                var distance = RandomHelper.Range(2, minionsRadius);

                // ensure we spawn more objects closer to the epicenter
                var spawnProbability = 1 - (distance / minionsRadius);
                spawnProbability = Math.Pow(spawnProbability, 1.25);
                if (!RandomHelper.RollWithProbability(spawnProbability))
                {
                    // random skip
                    continue;
                }

                var angle = RandomHelper.NextDouble() * MathConstants.DoublePI;
                var spawnPosition = new Vector2Ushort(
                    (ushort)(epicenterPosition.X + distance * Math.Cos(angle)),
                    (ushort)(epicenterPosition.Y + distance * Math.Sin(angle)));

                if (ServerTrySpawnMinion(spawnPosition))
                {
                    // spawned successfully!
                    countToSpawnRemains--;
                }
            }

            bool ServerTrySpawnMinion(Vector2Ushort spawnPosition)
            {
                var worldPosition = spawnPosition.ToVector2D();
                if (!ServerCharacterSpawnHelper.IsPositionValidForCharacterSpawn(worldPosition,
                                                                                 isPlayer: false))
                {
                    // position is not valid for spawning
                    return false;
                }

                var spawnedCharacter = ServerCharacters.SpawnCharacter(minionProto, worldPosition);
                return spawnedCharacter != null;
            }
        }

        private static void ServerNotifyCharactersEligibleForReward(
            IProtoCharacterMob protoCharacterBoss,
            List<WinnerEntry> list,
            ushort totalLootCount)
        {
            foreach (var entry in list)
            {
                Instance.CallClient(entry.Character,
                                    _ => _.ClientRemote_BossRewardAvailable(
                                        protoCharacterBoss,
                                        entry.DamagePercent,
                                        entry.LootCount,
                                        totalLootCount));
            }
        }

        private static void ServerNotifyCharactersNotEligibleForReward(
            IProtoCharacterMob protoCharacterBoss,
            List<ICharacter> list)
        {
            Instance.CallClient(list,
                                _ => _.ClientRemote_BossRewardNotAvailable(
                                    protoCharacterBoss));
        }

        private static List<(ICharacter Character, double Damage)> ServerSelectWinnersByDamage(
            IReadOnlyList<(ICharacter Character, double Damage)> allDamageByCharacter,
            int maxWinners)
        {
            var list = allDamageByCharacter.ToList();

            ApplyPercentileThreshold();

            if (list.Count > maxWinners)
            {
                list.RemoveRange(maxWinners,
                                 list.Count - maxWinners);
            }

            return list;

            void ApplyPercentileThreshold()
            {
                // There could be some players that dealt a very low damage.
                // Let's remove players that cumulatively dealt less than 10% of total damage to the boss.
                var thresholdTotalDamage = 0.9 * list.Sum(p => p.Damage);

                var accumulatorTotalDamage = 0.0;

                for (var index = 0; index < list.Count; index++)
                {
                    if (accumulatorTotalDamage >= thresholdTotalDamage)
                    {
                        // remove this and further entries as they've dealt too low damage
                        list.RemoveRange(index,
                                         list.Count - index);
                        break;
                    }

                    var damage = list[index].Damage;
                    accumulatorTotalDamage += damage;
                }
            }
        }

        /// <returns>Score value is in range from (0,1]</returns>
        private static IReadOnlyList<(ICharacter Character, double Score)> ServerSelectWinnersByParticipation(
            IReadOnlyList<(ICharacter Character, double Damage)> winnersByDamage)
        {
            var totalDamage = winnersByDamage.Sum(p => p.Damage);
            return winnersByDamage.Select(p => (p.Character, p.Damage / totalDamage))
                                  .ToList();
        }

        private void ClientRemote_BossRewardAvailable(
            IProtoCharacterMob protoCharacterBoss,
            byte damagePercent,
            byte lootCount,
            ushort totalLootCount)
        {
            NotificationSystem.ClientShowNotification(
                                  CoreStrings.Victory,
                                  string.Format(CoreStrings.EventDamage_SuccessMessage_Format,
                                                damagePercent,
                                                lootCount,
                                                totalLootCount),
                                  NotificationColor.Good,
                                  protoCharacterBoss.Icon)
                              .HideAfterDelay(60);
        }

        private void ClientRemote_BossRewardNotAvailable(
            IProtoCharacterMob protoCharacterBoss)
        {
            // TODO: this is not correct. There is no actual percent metric but a threshold.
            byte damagePercentRequired = 5;

            NotificationSystem.ClientShowNotification(
                                  CoreStrings.EventDamage_FailureTitle_Format,
                                  string.Format(CoreStrings.EventDamage_FailureMessage_Format,
                                                damagePercentRequired),
                                  NotificationColor.Neutral,
                                  protoCharacterBoss.Icon)
                              .HideAfterDelay(60);
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        private void ClientRemote_VictoryAnnouncement(
            IProtoCharacterMob protoCharacterBoss,
            (string Name, string ClanTag)[] winnerNamesWithClanTags)
        {
            NotificationSystem.ClientShowNotification(
                                  title: null,
                                  message: string.Format(ProtoEventBoss.Notification_VictoryAnnouncement_Format,
                                                         protoCharacterBoss.Name)
                                           + "[*]"
                                           + winnerNamesWithClanTags.Select(GetFormattedName)
                                                                    .GetJoinedString("[*]"),
                                  color: NotificationColor.Neutral,
                                  icon: protoCharacterBoss.Icon)
                              .HideAfterDelay(60);

            string GetFormattedName((string Name, string ClanTag) entry)
            {
                if (string.IsNullOrEmpty(entry.ClanTag))
                {
                    return entry.Name;
                }

                var result = string.Format(CoreStrings.ClanTag_FormatWithName, entry.ClanTag, entry.Name);
                // escape formatting
                return result.Replace("[", @"\[")
                             .Replace("]", @"\]");
            }
        }

        private readonly struct WinnerEntry
        {
            public readonly ICharacter Character;

            public readonly byte DamagePercent;

            public readonly byte LootCount;

            public WinnerEntry(ICharacter character, byte damagePercent, byte lootCount)
            {
                this.Character = character;
                this.DamagePercent = damagePercent;
                this.LootCount = lootCount;
            }
        }
    }
}