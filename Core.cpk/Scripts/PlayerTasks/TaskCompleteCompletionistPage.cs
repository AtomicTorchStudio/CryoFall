namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.CoreMod.Systems.Completionist;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class TaskCompleteCompletionistPage : BasePlayerTaskWithDefaultState
    {
        /// <summary>
        /// We're using this only for achievements tracking so it should not be visible to player.
        /// </summary>
        [NotLocalizable]
        public const string DescriptionFormat = "Complete: {0} (completionist)";

        private TaskCompleteCompletionistPage(CompletionistPageName completionistPageName)
            : base(description: null)
        {
            this.CompletionistPage = completionistPageName;
        }

        public CompletionistPageName CompletionistPage { get; }

        public override bool IsReversible => false;

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.CompletionistPage.GetDescription());

        public static TaskCompleteCompletionistPage Require(CompletionistPageName pageName)
        {
            return new(pageName);
        }

        /// <summary>
        /// The completionist page is completed when it has all entries claimed by player
        /// and the number of added entries matches the max number of entries for the page.
        /// </summary>
        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskState state)
        {
            var completionistData = CompletionistSystem.SharedGetCompletionistData(character);

            var claimedEntriesCount = 0;
            foreach (var entry in completionistData.GetPageEntries(this.CompletionistPage))
            {
                if (!((ICompletionistDataEntry)entry).IsRewardClaimed)
                {
                    // has an unclaimed entry
                    return false;
                }

                claimedEntriesCount++;
            }

            return claimedEntriesCount >= CompletionistSystem.GetPageTotalEntriesNumber(this.CompletionistPage);
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