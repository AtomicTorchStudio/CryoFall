namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Systems.Completionist;
    using AtomicTorch.CBND.CoreMod.UI;
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

        [NotPersistent]
        [NotNetworkAvailableAttribute]
        public enum CompletionistPageName : byte
        {
            [Description(CoreStrings.WindowCompletionist_TabFood)]
            Food = 0,

            [Description(CoreStrings.WindowCompletionist_TabCreatures)]
            Creatures = 1,

            [Description(CoreStrings.WindowCompletionist_TabLoot)]
            Loot = 2,

            [Description(CoreStrings.WindowCompletionist_TabFish)]
            Fish = 3
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
            IEnumerable pageEntries = this.CompletionistPage switch
            {
                CompletionistPageName.Food      => completionistData.ListFood,
                CompletionistPageName.Creatures => completionistData.ListMobs,
                CompletionistPageName.Loot      => completionistData.ListLoot,
                CompletionistPageName.Fish      => completionistData.ListFish,
                _                               => throw new ArgumentOutOfRangeException()
            };

            var claimedEntriesCount = 0;
            foreach (var entry in pageEntries)
            {
                if (!((ICompletionistDataEntry)entry).IsRewardClaimed)
                {
                    // has an unclaimed entry
                    return false;
                }

                claimedEntriesCount++;
            }

            var requiredPageEntriesCount = this.CompletionistPage switch
            {
                CompletionistPageName.Food      => CompletionistSystem.CompletionistAllFood.Count,
                CompletionistPageName.Creatures => CompletionistSystem.CompletionistAllMobs.Count,
                CompletionistPageName.Loot      => CompletionistSystem.CompletionistAllLoot.Count,
                CompletionistPageName.Fish      => CompletionistSystem.CompletionistAllFish.Count,
                _                               => throw new ArgumentOutOfRangeException()
            };

            return claimedEntriesCount >= requiredPageEntriesCount;
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