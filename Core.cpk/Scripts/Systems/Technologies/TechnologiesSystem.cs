namespace AtomicTorch.CBND.CoreMod.Systems.Technologies
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;

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

        public static void ServerInitCharacterTechnologies(PlayerCharacterTechnologies technologies)
        {
            var needToResetTheTechTree = false;
            var techNodes = technologies.Nodes;
            var techGroups = technologies.Groups;

            for (var index = 0; index < techNodes.Count; index++)
            {
                var techNode = techNodes[index];
                if (!(techNode is null))
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
                if (!(techGroup is null))
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
                ServerResetTechTreeAndRefundLearningPoints(technologies);
            }

            foreach (var techGroup in FindProtoEntities<TechGroup>())
            {
                if (techGroup.GroupRequirements.Count > 0)
                {
                    continue;
                }

                // add free group
                technologies.ServerAddGroup(techGroup);

                foreach (var techNode in techGroup.RootNodes)
                {
                    ProcessNode(techNode);
                }
            }

            void ProcessNode(TechNode techNode)
            {
                if (techNode.LearningPointsPrice > 0)
                {
                    return;
                }

                // add free node
                technologies.ServerAddNode(techNode);

                foreach (var dependentNode in techNode.DependentNodes)
                {
                    ProcessNode(dependentNode);
                }
            }
        }

        public static void ServerResetTechTreeAndRefundLearningPoints(PlayerCharacterTechnologies technologies)
        {
            var lpToRefund = 0;
            foreach (var techNode in technologies.Nodes)
            {
                lpToRefund += techNode.LearningPointsPrice;
            }

            technologies.Nodes.Clear();

            foreach (var techGroup in technologies.Groups)
            {
                lpToRefund += techGroup.LearningPointsPrice;
            }

            technologies.Groups.Clear();

            technologies.ServerRefundLearningPoints(lpToRefund);
            technologies.IsTechTreeChanged = true;
        }

        public static void ServerUnlockGroup(ICharacter character, TechGroup techGroup)
        {
            techGroup.SharedValidateCanUnlock(character, skipLearningPointsCheck: false);
            var technologies = character.SharedGetTechnologies();
            technologies.ServerRemoveLearningPoints(techGroup.LearningPointsPrice);
            technologies.ServerAddGroup(techGroup);
            technologies.IsTechTreeChanged = false;
        }

        public static void ServerUnlockNode(ICharacter character, TechNode techNode)
        {
            techNode.SharedValidateCanUnlock(character);
            var technologies = character.SharedGetTechnologies();
            technologies.ServerRemoveLearningPoints(techNode.LearningPointsPrice);
            technologies.ServerAddNode(techNode);
            technologies.IsTechTreeChanged = false;
        }

        private (double LearningPointsGainMultiplier,
            double TimeGameTier3Basic,
            double TimeGameTier3Specialized,
            double TimeGameTier4Basic,
            double TimeGameTier4Specialized) ServerRemote_RequestTechRates()
        {
            return (TechConstants.ServerLearningPointsGainMultiplier,
                    TechConstants.PvpTechTimeGameTier3Basic,
                    TechConstants.PvpTechTimeGameTier3Specialized,
                    TechConstants.PvpTechTimeGameTier4Basic,
                    TechConstants.PvpTechTimeGameTier4Specialized);
        }

        private void ServerRemote_UnlockGroup(TechGroup techGroup)
        {
            ServerUnlockGroup(ServerRemoteContext.Character, techGroup);
        }

        private void ServerRemote_UnlockNode(TechNode techNode)
        {
            ServerUnlockNode(ServerRemoteContext.Character, techNode);
        }

        // This bootstrapper requests tech-related rates from the server.
        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += Refresh;
                Refresh();

                static async void Refresh()
                {
                    if (Client.Characters.CurrentPlayerCharacter is null)
                    {
                        return;
                    }

                    var rates = await Instance.CallServer(
                                    _ => _.ServerRemote_RequestTechRates());
                    TechConstants.ClientSetLearningPointsGainMultiplier(rates.LearningPointsGainMultiplier);
                    TechConstants.ClientSetPvpTechTimeGame(rates.TimeGameTier3Basic,
                                                           rates.TimeGameTier3Specialized,
                                                           rates.TimeGameTier4Basic,
                                                           rates.TimeGameTier4Specialized);
                }
            }
        }
    }
}