namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskCompleteTechGroup : BasePlayerTaskWithDefaultState
    {
        public const string DescriptionFormat = "Complete: {0} (tech group)";

        private TaskCompleteTechGroup(TechGroup techGroup)
            : base(description: null)
        {
            this.TechGroup = techGroup;
        }

        public override bool IsReversible => true;

        public TechGroup TechGroup { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.TechGroup.NameWithTierName);

        public static TaskCompleteTechGroup Require<TTechGroup>()
            where TTechGroup : TechGroup, new()
        {
            var proto = Api.GetProtoEntity<TTechGroup>();
            return Require(proto);
        }

        public static TaskCompleteTechGroup Require(TechGroup proto)
        {
            return new TaskCompleteTechGroup(proto);
        }

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskState state)
        {
            var unlockedCount = 0;

            var unlockedTechNodes = character.SharedGetTechnologies().Nodes;
            foreach (var techNode in unlockedTechNodes)
            {
                if (ReferenceEquals(techNode.Group, this.TechGroup))
                {
                    unlockedCount++;
                }
            }

            return unlockedCount >= this.TechGroup.Nodes.Count;
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                PlayerCharacterTechnologies.ServerCharacterTechNodeAddedOrRemoved
                    += this.ServerCharacterTechNodeAddedOrRemovedHandler;
            }
            else
            {
                PlayerCharacterTechnologies.ServerCharacterTechNodeAddedOrRemoved
                    -= this.ServerCharacterTechNodeAddedOrRemovedHandler;
            }
        }

        private void ServerCharacterTechNodeAddedOrRemovedHandler(ICharacter character, TechNode techNode, bool isAdded)
        {
            if (!ReferenceEquals(techNode.Group, this.TechGroup))
            {
                return;
            }

            var context = this.GetActiveContext(character, out _);
            context?.Refresh();
        }
    }
}