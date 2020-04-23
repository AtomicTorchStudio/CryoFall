namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskHaveTechNode : BasePlayerTaskWithDefaultState
    {
        public const string DescriptionFormat = "Learn: {0} (in {1} tech group)";

        private TaskHaveTechNode(TechNode techNode)
            : base(description: null)
        {
            this.TechNode = techNode;
        }

        public override bool IsReversible => true;

        public TechNode TechNode { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat,
                             this.TechNode.Name,
                             this.TechNode.Group.Name);

        public static TaskHaveTechNode Require<TTechNode>()
            where TTechNode : TechNode, new()
        {
            var proto = Api.GetProtoEntity<TTechNode>();
            return new TaskHaveTechNode(proto);
        }

        public override ITextureResource ClientCreateIcon()
        {
            return this.TechNode.Icon;
        }

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskState state)
        {
            return character.SharedGetTechnologies()
                            .SharedIsNodeUnlocked(this.TechNode);
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                PlayerCharacterTechnologies.ServerCharacterTechNodeAddedOrRemoved +=
                    this.ServerCharacterTechNodeAddedOrRemovedHandler;
            }
            else
            {
                PlayerCharacterTechnologies.ServerCharacterTechNodeAddedOrRemoved -=
                    this.ServerCharacterTechNodeAddedOrRemovedHandler;
            }
        }

        private void ServerCharacterTechNodeAddedOrRemovedHandler(ICharacter character, TechNode techNode, bool isAdded)
        {
            if (techNode != this.TechNode)
            {
                return;
            }

            var context = this.GetActiveContext(character, out _);
            context?.Refresh();
        }
    }
}