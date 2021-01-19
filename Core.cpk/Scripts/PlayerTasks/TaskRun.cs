namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class TaskRun : BasePlayerTaskWithDefaultState
    {
        public const string DescriptionText = "Sprint for 10 meters";

        // a singleton requirement
        public static readonly TaskRun Require = new();

        private ServerWrappedTriggerTimeInterval serverUpdater;

        private TaskRun()
            : base(DescriptionText)
        {
        }

        public override bool IsReversible => false;

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskState state)
        {
            var input = PlayerCharacter.GetPublicState(character).AppliedInput;
            return (input.MoveModes & CharacterMoveModes.ModifierRun)
                   == CharacterMoveModes.ModifierRun;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                this.serverUpdater = new ServerWrappedTriggerTimeInterval(
                    this.ServerRefreshAllActiveContexts,
                    TimeSpan.FromSeconds(3),
                    "QuestRequirement.Run");
            }
            else
            {
                this.serverUpdater.Dispose();
                this.serverUpdater = null;
            }
        }
    }
}