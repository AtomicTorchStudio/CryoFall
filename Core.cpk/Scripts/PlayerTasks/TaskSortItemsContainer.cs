namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class TaskSortItemsContainer : BasePlayerTaskWithDefaultState
    {
        public const string DescriptionText = "Sort inventory items";

        // a singleton requirement
        public static readonly IPlayerTask Require
            = new TaskSortItemsContainer();

        private TaskSortItemsContainer()
            : base(DescriptionText)
        {
        }

        private static event Action<ICharacter> ServerItemsContainerSortedByPlayer;

        public override bool IsReversible => false;

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskState state)
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
            context?.SetCompleted();
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

                    var requirements = questEntry.Quest.Tasks;
                    for (var index = 0; index < requirements.Count; index++)
                    {
                        if (!(requirements[index] is TaskSortItemsContainer))
                        {
                            continue;
                        }

                        if (questEntry.TaskStates[index].IsCompleted)
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