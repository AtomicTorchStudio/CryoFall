namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class RequirementSortItemsContainer : QuestRequirementWithDefaultState
    {
        public const string DescriptionText = "Sort inventory items";

        // a singleton requirement
        public static readonly IQuestRequirement Require
            = new RequirementSortItemsContainer();

        private RequirementSortItemsContainer()
            : base(DescriptionText)
        {
        }

        private static event Action<ICharacter> ServerItemsContainerSortedByPlayer;

        public override bool IsReversible => false;

        protected override bool ServerIsSatisfied(ICharacter character, QuestRequirementState state)
        {
            // never refreshed by the server
            return false;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                ServerItemsContainerSortedByPlayer += this.ServerItemsContainerSortedByPlayerHandler;
            }
            else
            {
                ServerItemsContainerSortedByPlayer -= this.ServerItemsContainerSortedByPlayerHandler;
            }
        }

        private void ServerItemsContainerSortedByPlayerHandler(ICharacter character)
        {
            var context = this.GetActiveContext(character, out _);
            context?.SetSatisfied();
        }

        public class Helper : ProtoSystem<Helper>
        {
            [NotLocalizable]
            public override string Name => "Sort items helper";

            public static void ClientOnItemsContainerSorted()
            {
                var character = ClientCurrentCharacterHelper.Character;
                var quests = character.SharedGetQuests();

                foreach (var questEntry in quests.Quests)
                {
                    if (questEntry.IsCompleted)
                    {
                        continue;
                    }

                    var requirements = questEntry.Quest.Requirements;
                    for (var index = 0; index < requirements.Count; index++)
                    {
                        if (!(requirements[index] is RequirementSortItemsContainer))
                        {
                            continue;
                        }

                        if (questEntry.RequirementStates[index].IsSatisfied)
                        {
                            continue;
                        }

                        // found an unsatisfied items container sort requirement - notify the server
                        Instance.CallServer(_ => _.ServerRemote_OnItemsContainerSortedByPlayer());
                        return;
                    }
                }
            }

            private void ServerRemote_OnItemsContainerSortedByPlayer()
            {
                var character = ServerRemoteContext.Character;
                ServerItemsContainerSortedByPlayer?.Invoke(character);
            }
        }
    }
}