namespace AtomicTorch.CBND.CoreMod.Systems.Achievements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Achievements;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Extensions;

    [PrepareOrder(afterType: typeof(IProtoAchievement))]
    public class AchievementsSystem : ProtoSystem<AchievementsSystem>
    {
        public static IReadOnlyList<IProtoAchievement> AllAchievements;

        public static bool CanUnlockAchievements
        {
            get
            {
                var game = Client.CurrentGame;
                if (game.ConnectionState != ConnectionState.Connected
                    || game.ServerInfo is null)
                {
                    return false;
                }

                if (!game.ServerInfo.IsModded
                    || game.IsConnectedToOfficialServer
                    || game.IsConnectedToFeaturedServer
                    || game.ServerInfo.ServerAddress.IsLocalServer
                    || (SharedLocalServerHelper.ClientTaskIsLocalServerPropertyReceived.IsCompleted
                        && SharedLocalServerHelper.IsLocalServer))
                {
                    return true;
                }

                Api.Logger.Important(
                    "The client will not accept the achievements while playing on a modded server that is not official, featured, or local");
                return false;
            }
        }

        public static void ServerInitCharacterAchievements(PlayerCharacterAchievements characterAchievements)
        {
            // add all achievements as locked
            foreach (var protoAchievement in AllAchievements)
            {
                characterAchievements.ServerTryAddAchievement(protoAchievement, isUnlocked: false);
            }
        }

        protected override void PrepareSystem()
        {
            AllAchievements = Api.FindProtoEntities<IProtoAchievement>()
                                 .ToArray();
        }

        private class Bootstrapper : BaseBootstrapper
        {
            private static PlayerCharacterAchievements clientCurrentAchievements;

            public override void ClientInitialize()
            {
                BootstrapperClientGame.InitCallback += Refresh;
                SharedLocalServerHelper.IsLocalServerPropertyChanged += IsLocalServerPropertyChangedHandler;

                static void Refresh(ICharacter character)
                {
                    var newAchievements = PlayerCharacter.GetPrivateState(character)?.Achievements;
                    if (newAchievements is null
                        || clientCurrentAchievements == newAchievements)
                    {
                        return;
                    }

                    if (clientCurrentAchievements is not null)
                    {
                        var list = clientCurrentAchievements.UnlockedAchievements;
                        list.ClientElementInserted -= ClientUnlockedAchievementAdded;
                    }

                    clientCurrentAchievements = newAchievements;

                    if (clientCurrentAchievements is not null)
                    {
                        var list = clientCurrentAchievements.UnlockedAchievements;
                        list.ClientElementInserted += ClientUnlockedAchievementAdded;
                    }

                    Logger.Important("Achievements received from the server: "
                                     + Environment.NewLine
                                     + clientCurrentAchievements.UnlockedAchievements
                                                                .Select(a => a.Achievement.AchievementId)
                                                                .Select(a => "* " + a)
                                                                .GetJoinedString(Environment.NewLine));
                    ClientSyncAchievements();
                }

                static void IsLocalServerPropertyChangedHandler()
                {
                    ClientSyncAchievements();
                }
            }

            private static void ClientSyncAchievements()
            {
                if (clientCurrentAchievements is null
                    || clientCurrentAchievements.UnlockedAchievements.Count == 0)
                {
                    return;
                }

                var achievementIds = clientCurrentAchievements.UnlockedAchievements
                                                              .Select(a => a.Achievement.AchievementId)
                                                              .ToArray();

                if (CanUnlockAchievements)
                {
                    Logger.Important("Synchronizing Steam achievements: " + achievementIds.Length);
                    Client.SteamApi.UnlockAchievements(achievementIds);
                }
            }

            private static void ClientUnlockedAchievementAdded(
                NetworkSyncList<PlayerCharacterAchievements.CharacterAchievementEntry> source,
                int index,
                PlayerCharacterAchievements.CharacterAchievementEntry value)
            {
                var achievementId = value.Achievement.AchievementId;
                Logger.Important("Achievement added: " + achievementId);

                if (CanUnlockAchievements)
                {
                    Client.SteamApi.UnlockAchievement(achievementId);
                }
            }
        }
    }
}