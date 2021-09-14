namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.CoreMod.Systems.Completionist;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskCompleteCompletionistEntry : BasePlayerTaskWithDefaultState
    {
        /// <summary>
        /// We're using this only for achievements tracking so it should not be visible to player.
        /// </summary>
        [NotLocalizable]
        public const string DescriptionFormat = "Complete: {0} (completionist)";

        private TaskCompleteCompletionistEntry(
            IProtoEntity entryProtoEntity,
            CompletionistPageName completionistPage,
            bool isRewardClaimRequired)
            : base(description: null)
        {
            this.EntryProtoEntity = entryProtoEntity;
            this.CompletionistPage = completionistPage;
            this.IsRewardClaimRequired = isRewardClaimRequired;
        }

        public CompletionistPageName CompletionistPage { get; }

        public IProtoEntity EntryProtoEntity { get; }

        public override bool IsReversible => false;

        public bool IsRewardClaimRequired { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.EntryProtoEntity.Name);

        public static TaskCompleteCompletionistEntry Require<TProtoEntity>(bool isRewardClaimRequired = false)
            where TProtoEntity : IProtoEntity, new()
        {
            var protoEntity = Api.GetProtoEntity<TProtoEntity>();
            var pageName = CompletionistSystem.GetCompletionistPageName(protoEntity);
            return new(protoEntity, pageName, isRewardClaimRequired);
        }

        /// <summary>
        /// The completionist page is completed when it has all entries claimed by player
        /// and the number of added entries matches the max number of entries for the page.
        /// </summary>
        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskState state)
        {
            var completionistData = CompletionistSystem.SharedGetCompletionistData(character);
            foreach (var abstractEntry in completionistData.GetPageEntries(this.CompletionistPage))
            {
                var entry = ((ICompletionistDataEntry)abstractEntry);
                if (!ReferenceEquals(entry.Prototype, this.EntryProtoEntity))
                {
                    continue;
                }

                return !this.IsRewardClaimRequired
                       || entry.IsRewardClaimed;
            }

            return false;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                if (this.IsRewardClaimRequired)
                {
                    PlayerCharacterCompletionistData.ServerCharacterRewardClaimed
                        += this.ServerCharacterCompletionistEntryUnlockedOrClaimedHandler;
                }
                else
                {
                    PlayerCharacterCompletionistData.ServerCharacterEntryUnlocked
                        += this.ServerCharacterCompletionistEntryUnlockedOrClaimedHandler;
                }
            }
            else
            {
                if (this.IsRewardClaimRequired)
                {
                    PlayerCharacterCompletionistData.ServerCharacterRewardClaimed
                        -= this.ServerCharacterCompletionistEntryUnlockedOrClaimedHandler;
                }
                else
                {
                    PlayerCharacterCompletionistData.ServerCharacterEntryUnlocked
                        -= this.ServerCharacterCompletionistEntryUnlockedOrClaimedHandler;
                }
            }
        }

        private void ServerCharacterCompletionistEntryUnlockedOrClaimedHandler(
            ICharacter character,
            ICompletionistDataEntry entry)
        {
            var context = this.GetActiveContext(character, out _);
            context?.Refresh();
        }
    }
}