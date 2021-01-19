namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Completionist;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class TaskCompleteCompletionistAnyEntry : BasePlayerTaskWithDefaultState
    {
        public const string DefaultDescription = "Completionist: accept any reward";

        // a singleton requirement
        public static readonly TaskCompleteCompletionistAnyEntry Require = new();

        private TaskCompleteCompletionistAnyEntry() : base(DefaultDescription)
        {
        }

        public override bool IsReversible => false;

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskState state)
        {
            var completionistData = CompletionistSystem.SharedGetCompletionistData(character);
            return HasAny(completionistData.ListFood)
                   || HasAny(completionistData.ListMobs)
                   || HasAny(completionistData.ListLoot)
                   || HasAny(completionistData.ListFish);

            static bool HasAny<T>(IReadOnlyList<T> pageEntries)
                where T : ICompletionistDataEntry
            {
                foreach (var entry in pageEntries)
                {
                    if (entry.IsRewardClaimed)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                PlayerCharacterCompletionistData.ServerCharacterRewardClaimed
                    += this.ServerCharacterCompletionistRewardClaimed;
            }
            else
            {
                PlayerCharacterCompletionistData.ServerCharacterRewardClaimed
                    -= this.ServerCharacterCompletionistRewardClaimed;
            }
        }

        private void ServerCharacterCompletionistRewardClaimed(ICharacter character, ICompletionistDataEntry entry)
        {
            var context = this.GetActiveContext(character, out _);
            context?.Refresh();
        }
    }
}