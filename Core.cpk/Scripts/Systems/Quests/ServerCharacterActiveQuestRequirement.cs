namespace AtomicTorch.CBND.CoreMod.Systems.Quests
{
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    /// <summary>
    /// Please note - this a non-serializable class.
    /// </summary>
    public class ServerCharacterActiveQuestRequirement
    {
        private bool isActive;

        public ServerCharacterActiveQuestRequirement(
            PlayerCharacterQuests.CharacterQuestEntry questEntry,
            byte questRequirementIndex,
            ICharacter character)
        {
            this.QuestEntry = questEntry;
            this.QuestRequirementIndex = questRequirementIndex;
            this.Character = character;
        }

        public ICharacter Character { get; }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;
                //Api.Logger.Important("Active quest requirement status changed: " + this, this.Character);
                this.QuestRequirement.ServerRegisterOrUnregisterContext(this);
            }
        }

        public PlayerCharacterQuests.CharacterQuestEntry QuestEntry { get; }

        public IQuestRequirement QuestRequirement
            => this.QuestEntry.Quest.Requirements[this.QuestRequirementIndex];

        public int QuestRequirementIndex { get; }

        public QuestRequirementState QuestRequirementState
            => this.QuestEntry.RequirementStates[this.QuestRequirementIndex];

        public void Refresh()
        {
            var requirement = this.QuestRequirement;
            var state = this.QuestRequirementState;
            if (state.IsSatisfied
                && !requirement.IsReversible)
            {
                return;
            }

            var wasSatisfied = state.IsSatisfied;
            requirement.ServerRefreshIsSatisfied(this.Character, state);

            if (state.IsSatisfied != wasSatisfied)
            {
                this.QuestEntry.OnActiveRequirementSatisfiedStateChanged(this);
            }
        }

        public void SetSatisfied()
        {
            var state = this.QuestRequirementState;
            if (state.IsSatisfied)
            {
                return;
            }

            state.IsSatisfied = true;
            this.QuestEntry.OnActiveRequirementSatisfiedStateChanged(this);
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}, {2}: {3}, {4}: {5}, {6}: {7}, {8}: {9}",
                                 nameof(this.Character),
                                 this.Character,
                                 nameof(this.QuestEntry.Quest),
                                 this.QuestEntry.Quest.ShortId,
                                 nameof(this.QuestRequirementIndex),
                                 this.QuestRequirementIndex,
                                 nameof(this.QuestRequirement),
                                 this.QuestRequirement,
                                 nameof(this.IsActive),
                                 this.IsActive);
        }
    }
}