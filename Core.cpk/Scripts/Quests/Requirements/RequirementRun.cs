namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class RequirementRun : QuestRequirementWithDefaultState
    {
        public const string DescriptionText = "Sprint for 10 meters";

        // a singleton requirement
        public static readonly IQuestRequirement Require
            = new RequirementRun();

        private ServerWrappedTriggerTimeInterval serverUpdater;

        private RequirementRun()
            : base(DescriptionText)
        {
        }

        public override bool IsReversible => false;

        protected override bool ServerIsSatisfied(ICharacter character, QuestRequirementState state)
        {
            if (!character.IsOnline)
            {
                return false;
            }

            // check that player character has a running modifier active
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