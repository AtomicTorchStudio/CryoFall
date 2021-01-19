namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    /// <summary>
    /// This system is keeping an eye on PvP players that are playing in public factions.
    /// 
    /// The problem is that anyone can join a public faction, but they cannot be managed 24/7
    /// so some players may want to abuse the game by killing friendly players in a surprise attack.
    /// When anyone kills a friendly player (their own faction's member or an ally faction's member),
    /// their karma drops on some value. If the player kills a significant number of friendly players
    /// in a relatively short duration, the karma will drop too much and this player
    /// will be automatically kicked from their faction by karma system.
    /// 
    /// Party members could kill each other without consequences even if they're in the same faction.
    /// 
    /// The karma is restored slowly so players that only accidentally kill friendly players
    /// will not suffer from it, but any abuser would be kicked quickly.
    /// 
    /// Technically, we're not calling it karma in the game texts that players could see
    /// (we rather prefer "faction standing" or reputation as the game has a sci-fi setting),
    /// but karma system is the standard name for this game mechanic.
    /// </summary>
    public class FactionKarmaSystem : ProtoSystem<FactionKarmaSystem>
    {
        /// <summary>
        /// Kick player from a public faction when the karma drops below this level.
        /// </summary>
        public const double KarmaFactionKickThreshold = -80;

        /// <summary>
        /// Drop player's karma points when the system detects that
        /// a friendly player has died due to the induced damage.
        /// </summary>
        public const double KarmaPenaltyPerKill = 50;

        /// <summary>
        /// Karma restore speed (per hour).
        /// </summary>
        public const double KarmaRestorePerUpdate = 1; // one karma point per update

        /// <summary>
        /// Karma restore interval.
        /// </summary>
        public const uint ServerUpdateInterval = 60 * 60; // update every hour

        private const string DatabaseKeyLastUpdateTime = "LastUpdateTime";

        private static double serverLastUpdateTime;

        [NotLocalizable]
        public override string Name => "Faction karma system";

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            if (PveSystem.ServerIsPvE)
            {
                // there is no need for faction karma system on PvE servers
                return;
            }

            if (!Server.Database.TryGet(nameof(FactionLeaderboardSystem),
                                        DatabaseKeyLastUpdateTime,
                                        out serverLastUpdateTime))
            {
                serverLastUpdateTime = Server.Game.FrameTime;
                Server.Database.Set(nameof(FactionLeaderboardSystem),
                                    DatabaseKeyLastUpdateTime,
                                    serverLastUpdateTime);
            }

            FactionSystem.ServerCharacterJoinedOrLeftFaction += ServerCharacterJoinedOrLeftFactionHandler;
            ServerCharacterDeathMechanic.CharacterDeath += ServerCharacterDeathHandler;

            TriggerTimeInterval.ServerConfigureAndRegister(
                TimeSpan.FromSeconds(1),
                ServerUpdate,
                "System." + this.ShortId);
        }

        /// <summary>
        /// Check whether it was a player character killed and check its damage sources history.
        /// If it was killed by a friendly faction member, punish that member by reducing its karma
        /// and kick if it's too low.
        /// Please note: party members could kill each other without consequences even if they're in the same faction.
        /// </summary>
        private static void ServerCharacterDeathHandler(ICharacter killedCharacter)
        {
            if (killedCharacter.IsNpc)
            {
                return;
            }

            // get damage sources for the past 12.5 seconds (2.5 chunks)
            var damageSources = CharacterDamageTrackingSystem.ServerGetDamageSourcesAfterTime(
                killedCharacter,
                afterTime: Server.Game.FrameTime
                           - ServerCharacterDamageSourcesStats.ChunkDuration * 2.5);
            if (damageSources is null)
            {
                return;
            }

            var killedCharacterFaction = FactionSystem.ServerGetFaction(killedCharacter);
            if (killedCharacterFaction is null)
            {
                // a non-faction player character was killed - no karma system action
                return;
            }

            foreach (var entry in damageSources)
            {
                if (entry.ProtoEntity is not PlayerCharacter)
                {
                    continue;
                }

                var attackerCharacter = Server.Characters.GetPlayerCharacter(entry.Name);
                if (attackerCharacter is null)
                {
                    // should not be possible
                    continue;
                }

                ProcessAttackerCharacter(attackerCharacter, damageFraction: entry.Fraction);
            }

            void ProcessAttackerCharacter(ICharacter attackerCharacter, double damageFraction)
            {
                // found a player character that has damaged the (now dead) player character recently
                if (PartySystem.ServerIsSameParty(killedCharacter, attackerCharacter))
                {
                    // damaging your own party members is fine
                    return;
                }

                var attackerCharacterFaction = FactionSystem.ServerGetFaction(attackerCharacter);
                if (attackerCharacterFaction is null
                    || Faction.GetPublicState(attackerCharacterFaction).Kind != FactionKind.Public)
                {
                    // attacking character is not a member of a public faction - no karma system action
                    return;
                }

                if (!ReferenceEquals(killedCharacterFaction, attackerCharacterFaction)
                    && (FactionSystem.ServerGetFactionDiplomacyStatus(killedCharacterFaction,
                                                                      attackerCharacterFaction)
                        != FactionDiplomacyStatus.Ally))
                {
                    // the killer is not a member of the same faction, and not an ally faction's member
                    return;
                }

                // a friendly player character has been killed, apply the punishment
                var attackerCharacterPrivateState = PlayerCharacter.GetPrivateState(attackerCharacter);
                var karmaPenalty = KarmaPenaltyPerKill * Math.Min(1, damageFraction * 2.0);
                attackerCharacterPrivateState.ServerFactionKarma -= karmaPenalty;
                Logger.Info(
                    string.Format(
                        "{0} karma decreased by {1:0.##} to {2:0.##} - due to {3:0.##}% recent damage to killed character {4}",
                        attackerCharacter,
                        karmaPenalty,
                        attackerCharacterPrivateState.ServerFactionKarma,
                        Math.Round(damageFraction * 100),
                        killedCharacter));

                // notify about the decreased karma
                Instance.CallClient(attackerCharacter,
                                    _ => _.ClientRemote_OnKarmaDecreased());

                if (attackerCharacterPrivateState.ServerFactionKarma > KarmaFactionKickThreshold)
                {
                    // didn't reach the kick threshold                    
                    return;
                }

                // that's enough! Bye-bye, killer!
                if (FactionSystem.ServerGetRole(attackerCharacter) == FactionMemberRole.Leader)
                {
                    if (FactionSystem.ServerGetFactionMembersReadOnly(attackerCharacterFaction).Count == 1)
                    {
                        // we cannot kick a faction leader as it's the only member in a faction
                        // but it also means that it didn't kill a faction member but an ally faction's member
                        // just drop the alliance
                        Logger.Important(
                            string.Format(
                                "Leader of the public faction [{0}] with just a single member ({1}) killed a member ({2}) of an ally faction [{3}] and reached a too low karma level - break the alliance",
                                FactionSystem.SharedGetClanTag(attackerCharacterFaction),
                                attackerCharacter,
                                killedCharacter,
                                FactionSystem.SharedGetClanTag(killedCharacterFaction)));

                        FactionSystem.ServerSetFactionDiplomacyStatusNeutral(killedCharacterFaction,
                                                                             attackerCharacterFaction);
                        // reset the karma a bit - restore an equivalent of a half of the karma penalty for killing
                        attackerCharacterPrivateState.ServerFactionKarma += KarmaPenaltyPerKill / 2.0;
                        return;
                    }

                    // pass the faction ownership to a different faction member and kick the killer character
                    var newLeaderName = FactionSystem.ServerGetFactionMembersReadOnly(attackerCharacterFaction)
                                                     .FirstOrDefault(m => m.Role != FactionMemberRole.Member
                                                                          && m.Role != FactionMemberRole.Leader)
                                                     .Name;

                    if (string.IsNullOrEmpty(newLeaderName))
                    {
                        newLeaderName = FactionSystem.ServerGetFactionMembersReadOnly(attackerCharacterFaction)
                                                     .First(m => m.Role != FactionMemberRole.Leader)
                                                     .Name;
                    }

                    Logger.Important(
                        string.Format(
                            "Leader of the public faction [{0}] - {1} - killed a friendly player ({2}) and reached a too low karma level - ownership of the faction will be passed to {3}. Player {1} will be kicked from the faction.",
                            FactionSystem.SharedGetClanTag(attackerCharacterFaction),
                            attackerCharacter,
                            killedCharacter,
                            newLeaderName));

                    // It may case a warning message on the client side
                    // as the faction leader is later removed from the faction
                    // but it expects to receive the updated faction information.
                    FactionSystem.ServerTransferFactionOwnership(attackerCharacter, newLeaderName);
                }

                Logger.Important(
                    string.Format(
                        "Member of a public faction [{0}] - {1} - killed a friendly player ({2} from [{3}]) and reached a too low karma level. Player {1} will be kicked from the faction.",
                        FactionSystem.SharedGetClanTag(attackerCharacterFaction),
                        attackerCharacter,
                        killedCharacter,
                        FactionSystem.SharedGetClanTag(killedCharacterFaction)));

                FactionSystem.ServerRemoveMemberFromCurrentFaction(attackerCharacter);

                Instance.CallClient(attackerCharacter,
                                    _ => _.ClientRemote_OnKickedFromTheFactionDueToLowKarma());
            }
        }

        /// <summary>
        /// Resets the faction karma when player joins or leaves a faction.
        /// As player cannot rejoin the faction easily (they need an invitation to rejoin and a cooldown applies),
        /// it's not possible to workaround the karma system by leaving a faction and rejoining it back.
        /// </summary>
        private static void ServerCharacterJoinedOrLeftFactionHandler(
            ICharacter character,
            ILogicObject faction,
            bool isJoined)
        {
            PlayerCharacter.GetPrivateState(character).ServerFactionKarma = 0;
        }

        private static void ServerUpdate()
        {
            if (Api.Server.Game.FrameTime < serverLastUpdateTime + ServerUpdateInterval)
            {
                return;
            }

            // time for updating the faction karma for each player
            ServerUpdateKarma();
        }

        private static void ServerUpdateKarma()
        {
            var stopwatch = Stopwatch.StartNew();
            serverLastUpdateTime = Api.Server.Game.FrameTime;
            Server.Database.Set(nameof(FactionLeaderboardSystem),
                                DatabaseKeyLastUpdateTime,
                                serverLastUpdateTime);

            foreach (var playerCharacter in Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: false))
            {
                var privateState = PlayerCharacter.GetPrivateState(playerCharacter);
                var newKarma = privateState.ServerFactionKarma + KarmaRestorePerUpdate;
                privateState.ServerFactionKarma = newKarma >= 0
                                                      ? 0
                                                      : newKarma;
            }

            Logger.Important($"Faction karma update finished. It took: {stopwatch.Elapsed.TotalSeconds:F3}s.");
        }

        private void ClientRemote_OnKarmaDecreased()
        {
            NotificationSystem.ClientShowNotification(
                                  title: CoreStrings.Faction_NotificationCurrentPlayerStandingDecreased_Title,
                                  message: CoreStrings.Faction_NotificationCurrentPlayerStandingDecreased_Description
                                           + "[br]"
                                           + CoreStrings.Faction_OffendingPlayersAutomaticallyRemoved,
                                  NotificationColor.Bad)
                              .HideAfterDelay(60);
        }

        private void ClientRemote_OnKickedFromTheFactionDueToLowKarma()
        {
            NotificationSystem.ClientShowNotification(
                                  title: CoreStrings.Faction_Title,
                                  message: CoreStrings.Faction_NotificationCurrentPlayerRemoved_LowStanding
                                           + "[br]"
                                           + CoreStrings.Faction_OffendingPlayersAutomaticallyRemoved,
                                  NotificationColor.Bad)
                              .HideAfterDelay(60);
        }
    }
}