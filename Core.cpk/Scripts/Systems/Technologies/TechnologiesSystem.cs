namespace AtomicTorch.CBND.CoreMod.Systems.Technologies
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class TechnologiesSystem : ProtoSystem<TechnologiesSystem>
    {
        public const string NotificationCannotUnlockTech = "Cannot unlock tech";

        public override string Name => "Technologies system";

        public static void ClientUnlockGroup(TechGroup techGroup)
        {
            if (!ClientValidateCanUnlock(techGroup, showErrorNotification: true))
            {
                return;
            }

            Instance.CallServer(_ => _.ServerRemote_UnlockGroup(techGroup));
        }

        public static void ClientUnlockNode(TechNode techNode)
        {
            if (!techNode.SharedCanUnlock(Client.Characters.CurrentPlayerCharacter, out var error))
            {
                NotificationSystem.ClientShowNotification(NotificationCannotUnlockTech,
                                                          error,
                                                          NotificationColor.Bad);
                return;
            }

            Instance.CallServer(_ => _.ServerRemote_UnlockNode(techNode));
        }

        public static bool ClientValidateCanUnlock(
            TechGroup techGroup,
            bool showErrorNotification)
        {
            if (techGroup.SharedCanUnlock(Client.Characters.CurrentPlayerCharacter,
                                          skipLearningPointsCheck: false,
                                          out var error))
            {
                return true;
            }

            if (showErrorNotification)
            {
                NotificationSystem.ClientShowNotification(NotificationCannotUnlockTech,
                                                          error,
                                                          NotificationColor.Bad);
            }

            return false;
        }

        /// <summary>
        /// This method ensures that free tech groups are added automatically.
        /// </summary>
        public static void ServerEnsureFreeTechGroupsUnlocked(PlayerCharacterTechnologies technologies)
        {
            foreach (var techGroup in TechGroup.AvailableTechGroups)
            {
                if (techGroup.LearningPointsPrice > 0
                    || techGroup.GroupRequirements.Count > 0)
                {
                    continue;
                }

                technologies.ServerAddGroup(techGroup);
            }
        }

        public static void ServerInitCharacterTechnologies(PlayerCharacterTechnologies technologies)
        {
            var needToResetTheTechTree = false;
            var techNodes = technologies.Nodes;
            var techGroups = technologies.Groups;

            for (var index = 0; index < techNodes.Count; index++)
            {
                var techNode = techNodes[index];
                if (techNode is not null)
                {
                    continue;
                }

                // tech node not found
                needToResetTheTechTree = true;
                techNodes.RemoveAt(index);
                index--;
            }

            for (var index = 0; index < techGroups.Count; index++)
            {
                var techGroup = techGroups[index];
                if (techGroup is not null)
                {
                    continue;
                }

                // tech group not found
                needToResetTheTechTree = true;
                techGroups.RemoveAt(index);
                index--;
            }

            if (needToResetTheTechTree)
            {
                technologies.ServerResetTechTreeAndRefundLearningPoints();
            }

            ServerEnsureFreeTechGroupsUnlocked(technologies);
        }

        public static void ServerUnlockGroup(ICharacter character, TechGroup techGroup)
        {
            techGroup.SharedValidateCanUnlock(character, skipLearningPointsCheck: false);
            var technologies = character.SharedGetTechnologies();
            technologies.ServerRemoveLearningPoints(techGroup.LearningPointsPrice);
            technologies.ServerAddGroup(techGroup);
            technologies.IsTechTreeChanged = false;

            foreach (var rootNode in techGroup.RootNodes)
            {
                technologies.ServerAddNode(rootNode);
            }
        }

        public static void ServerUnlockNode(ICharacter character, TechNode techNode)
        {
            techNode.SharedValidateCanUnlock(character);
            var technologies = character.SharedGetTechnologies();
            technologies.ServerRemoveLearningPoints(techNode.LearningPointsPrice);
            technologies.ServerAddNode(techNode);
            technologies.IsTechTreeChanged = false;
        }

        private void ServerRemote_UnlockGroup(TechGroup techGroup)
        {
            ServerUnlockGroup(ServerRemoteContext.Character, techGroup);
        }

        private void ServerRemote_UnlockNode(TechNode techNode)
        {
            ServerUnlockNode(ServerRemoteContext.Character, techNode);
        }
    }
}