namespace AtomicTorch.CBND.CoreMod.Systems.Achievements
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Achievements;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    [PrepareOrder(afterType: typeof(IProtoAchievement))]
    public class AchievementsSystem : ProtoSystem<AchievementsSystem>
    {
        public static IReadOnlyList<IProtoAchievement> AllAchievements;

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

                void Refresh(ICharacter character)
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
                Client.SteamApi.UnlockAchievements(achievementIds);
                Logger.Important("Achievements received from the server: "
                                 + Environment.NewLine
                                 + achievementIds.Select(a => "* " + a)
                                                 .GetJoinedString(Environment.NewLine));
            }

            private static void ClientUnlockedAchievementAdded(
                NetworkSyncList<PlayerCharacterAchievements.CharacterAchievementEntry> source,
                int index,
                PlayerCharacterAchievements.CharacterAchievementEntry value)
            {
                var achievementId = value.Achievement.AchievementId;
                Client.SteamApi.UnlockAchievement(achievementId);
                Logger.Important("Achievement added: " + achievementId);
            }
        }
    }
}