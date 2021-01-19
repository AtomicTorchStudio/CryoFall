namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Quests.Data
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using JetBrains.Annotations;

    public class ViewModelQuestRequirement : BaseViewModel
    {
        private static readonly SoundResource SoundResourceQuestRequirementSatisfied =
            new("UI/Quests/QuestRequirementSatisfied.ogg");

        private static ulong lastTaskFinishedFrameNumber;

        private readonly IPlayerTask requirement;

        private readonly bool showIcon;

        private readonly PlayerTaskState taskState;

        private readonly PlayerTaskStateWithCount taskStateWithCount;

        public ViewModelQuestRequirement(
            IPlayerTask requirement,
            // can be null when the quest is completed
            [CanBeNull] PlayerTaskState taskState,
            bool showIcon)
        {
            this.requirement = requirement;
            this.taskState = taskState;
            this.showIcon = showIcon;

            this.taskState?.ClientSubscribe(
                _ => _.IsCompleted,
                isCompletedNow =>
                {
                    this.NotifyPropertyChanged(nameof(this.IsCompleted));
                    if (!isCompletedNow)
                    {
                        return;
                    }

                    // play requirement satisfied sound (not more often than once per frame)
                    if (lastTaskFinishedFrameNumber != Client.CurrentGame.ServerFrameNumber)
                    {
                        lastTaskFinishedFrameNumber = Client.CurrentGame.ServerFrameNumber;
                        Api.Client.Audio.PlayOneShot(SoundResourceQuestRequirementSatisfied, volume: 0.5f);
                    }
                },
                this);

            if (requirement is IPlayerTaskWithCount questRequirementWithCount)
            {
                this.CountRequired = questRequirementWithCount.RequiredCount;

                if (taskState is not null)
                {
                    // requirement state can be null if the quest is already completed
                    this.taskStateWithCount = (PlayerTaskStateWithCount)taskState;
                    this.taskStateWithCount.ClientSubscribe(
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
                if (this.taskStateWithCount is not null)
                {
                    return this.taskStateWithCount.CountCurrent;
                }

                if (this.taskState is null)
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

        public bool HasIcon => this.showIcon
                               && this.requirement.Icon is not null;

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.requirement.Icon);

        public bool IsCompleted => this.taskState?.IsCompleted ?? true;
    }
}