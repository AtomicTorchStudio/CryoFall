namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskHaveTechGroup : BasePlayerTaskWithDefaultState
    {
        public const string DescriptionFormat = "Unlock {0} (tech group)";

        private TaskHaveTechGroup(TechGroup techGroup)
            : base(description: null)
        {
            this.TechGroup = techGroup;
        }

        public override bool IsReversible => true;

        public TechGroup TechGroup { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.TechGroup.Name);

        public static TaskHaveTechGroup Require<TTechGroup>()
            where TTechGroup : TechGroup, new()
        {
            var proto = Api.GetProtoEntity<TTechGroup>();
            return new TaskHaveTechGroup(proto);
        }

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskState state)
        {
            return character.SharedGetTechnologies()
                            .SharedIsGroupUnlocked(this.TechGroup);
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                PlayerCharacterTechnologies.ServerCharacterGroupAddedOrRemoved += this.ServerCharacterGroupAddedOrRemovedHandler;
            }
            else
            {
                PlayerCharacterTechnologies.ServerCharacterGroupAddedOrRemoved -= this.ServerCharacterGroupAddedOrRemovedHandler;
            }
        }

        private void ServerCharacterGroupAddedOrRemovedHandler(ICharacter character, TechGroup techGroup, bool isAdded)
        {
            if (techGroup != this.TechGroup)
            {
                return;
            }

            var context = this.GetActiveContext(character, out _);
            context?.Refresh();
        }
    }
}