namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Quests.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using JetBrains.Annotations;

    public class ViewModelQuestRequirement : BaseViewModel
    {
        private readonly IQuestRequirement requirement;

        private readonly QuestRequirementState requirementState;

        private readonly QuestRequirementStateWithCount requirementStateWithCount;

        public ViewModelQuestRequirement(
            IQuestRequirement requirement,
            // can be null when the quest is completed
            [CanBeNull] QuestRequirementState requirementState)
        {
            this.requirement = requirement;
            this.requirementState = requirementState;

            this.requirementState?.ClientSubscribe(
                _ => _.IsSatisfied,
                isSatisfiedNow =>
                {
                    this.NotifyPropertyChanged(nameof(this.IsSatisfied));
                    if (isSatisfiedNow)
                    {
                        Api.Client.Audio.PlayOneShot(new SoundResource("UI/Quests/QuestRequirementSatisfied.ogg"));
                    }
                },
                this);

            if (requirement is IQuestRequirementWithCount questRequirementWithCount)
            {
                this.CountRequired = questRequirementWithCount.RequiredCount;

                if (requirementState != null)
                {
                    // requirement state can be null if the quest is already completed
                    this.requirementStateWithCount = (QuestRequirementStateWithCount)requirementState;
                    this.requirementStateWithCount.ClientSubscribe(
                        _ => _.CountCurrent,
                        _ => this.NotifyPropertyChanged(nameof(this.CountCurrent)),
                        this);
                }
            }
        }

        public ushort CountCurrent
        {
            get
            {
                if (this.requirementStateWithCount != null)
                {
                    return this.requirementStateWithCount.CountCurrent;
                }

                if (this.requirementState == null)
                {
                    // the quest is completed so there is no requirement state
                    return this.CountRequired;
                }

                return 0;
            }
        }

        public ushort CountRequired { get; }

        public Visibility CountRequirementVisibility =>
            this.CountRequired > 1
                ? Visibility.Visible
                : Visibility.Collapsed;

        public string Description => this.requirement.Description;

        public bool IsSatisfied => this.requirementState?.IsSatisfied ?? true;
    }
}