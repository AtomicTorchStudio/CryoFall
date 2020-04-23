namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Quests.Data
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelQuestEntry : BaseViewModel
    {
        private readonly bool showRequirementIcons;

        public ViewModelQuestEntry(
            PlayerCharacterQuests.CharacterQuestEntry questEntry,
            Action<ViewModelQuestEntry> callbackOnFinishedStateChanged,
            bool showRequirementIcons = true)
        {
            this.showRequirementIcons = showRequirementIcons;
            this.QuestEntry = questEntry;

            var requirements = questEntry.Quest.Tasks;
            var viewModels = new ViewModelQuestRequirement[requirements.Count];
            for (var index = 0; index < requirements.Count; index++)
            {
                var requirement = requirements[index];
                var requirementState = questEntry.IsCompleted
                                           ? null
                                           : questEntry.TaskStates[index];

                viewModels[index] = new ViewModelQuestRequirement(requirement,
                                                                  requirementState,
                                                                  showIcon: showRequirementIcons);
            }

            this.Requirements = viewModels;

            this.QuestEntry.ClientSubscribe(
                _ => _.AreAllTasksCompleted,
                _ =>
                {
                    this.NotifyPropertyChanged(nameof(this.AreAllTasksCompleted));
                    this.NotifyPropertyChanged(nameof(this.IsRewardCanBeClaimed));
                    this.NotifyPropertyChanged(nameof(this.IsNew));
                    callbackOnFinishedStateChanged(this);
                },
                this);

            this.QuestEntry.ClientSubscribe(
                _ => _.IsCompleted,
                _ =>
                {
                    this.NotifyPropertyChanged(nameof(this.IsCompletedAndRewardClaimed));
                    this.NotifyPropertyChanged(nameof(this.IsRewardCanBeClaimed));
                    this.NotifyPropertyChanged(nameof(this.IsNew));
                    // this is required as the quest is completely finished when the reward is claimed
                    callbackOnFinishedStateChanged(this);
                },
                this);

            this.QuestEntry.ClientSubscribe(
                _ => _.IsNew,
                _ => this.NotifyPropertyChanged(nameof(this.IsNew)),
                this);

            TechConstants.ClientLearningPointsGainMultiplierReceived +=
                this.ClientLearningPointsGainMultiplierReceivedHandler;
        }

        public bool AreAllTasksCompleted => this.QuestEntry.AreAllTasksCompleted;

        public BaseCommand CommandClaimReward => new ActionCommand(this.ExecuteCommandClaimReward);

        public BaseCommand CommandToggleCollapsed => new ActionCommand(this.ExecuteCommandToggleCollapsed);

        public string Description => this.QuestEntry.Quest.Description;

        public string Hints => this.QuestEntry.Quest.Hints;

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.QuestEntry.Quest.Icon);

        public bool IsCollapsed { get; private set; } = true;

        public bool IsCompletedAndRewardClaimed => this.QuestEntry.IsCompleted;

        public bool IsNew => this.QuestEntry.IsNew
                             && !this.IsRewardCanBeClaimed
                             && !this.IsCompletedAndRewardClaimed;

        public bool IsRewardCanBeClaimed => this.AreAllTasksCompleted
                                            && !this.IsCompletedAndRewardClaimed;

        public PlayerCharacterQuests.CharacterQuestEntry QuestEntry { get; }

        public float RequiredHeight { get; set; }

        public IReadOnlyList<ViewModelQuestRequirement> Requirements { get; }

        public ushort RewardLearningPoints
            => (ushort)Math.Round(this.QuestEntry.Quest.RewardLearningPoints
                                  * TechConstants.ClientLearningPointsGainMultiplier,
                                  MidpointRounding.AwayFromZero);

        public string Title => this.QuestEntry.Quest.Name;

        public void RemoveNewFlag()
        {
            if (this.QuestEntry.IsNew)
            {
                QuestsSystem.ClientMarkAsNotNew(this.QuestEntry.Quest);
            }
        }

        public void SetIsCollapsed(bool isCollapsed, bool removeNewFlag = true)
        {
            if (this.IsCollapsed == isCollapsed)
            {
                return;
            }

            this.IsCollapsed = isCollapsed;
            this.NotifyPropertyChanged(nameof(this.IsCollapsed));

            if (removeNewFlag)
            {
                this.RemoveNewFlag();
            }
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            TechConstants.ClientLearningPointsGainMultiplierReceived -=
                this.ClientLearningPointsGainMultiplierReceivedHandler;
        }

        private void ClientLearningPointsGainMultiplierReceivedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.RewardLearningPoints));
        }

        private void ExecuteCommandClaimReward()
        {
            QuestsSystem.ClientClaimReward(this.QuestEntry.Quest);
        }

        private void ExecuteCommandToggleCollapsed()
        {
            this.SetIsCollapsed(!this.IsCollapsed);
        }
    }
}