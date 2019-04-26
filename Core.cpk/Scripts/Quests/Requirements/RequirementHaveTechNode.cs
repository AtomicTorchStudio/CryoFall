namespace AtomicTorch.CBND.CoreMod.Quests
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementHaveTechNode : QuestRequirementWithDefaultState
    {
        public const string DescriptionFormat = "Learn: {0} (in {1} tech group)";

        private RequirementHaveTechNode(TechNode techNode)
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

        public static RequirementHaveTechNode Require<TTechNode>()
            where TTechNode : TechNode, new()
        {
            var proto = Api.GetProtoEntity<TTechNode>();
            return new RequirementHaveTechNode(proto);
        }

        protected override bool ServerIsSatisfied(ICharacter character, QuestRequirementState state)
        {
            return character.SharedGetTechnologies()
                            .SharedIsNodeUnlocked(this.TechNode);
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                PlayerCharacterTechnologies.CharacterTechNodeAddedOrRemoved +=
                    this.CharacterTechNodeAddedOrRemovedHandler;
            }
            else
            {
                PlayerCharacterTechnologies.CharacterTechNodeAddedOrRemoved -=
                    this.CharacterTechNodeAddedOrRemovedHandler;
            }
        }

        private void CharacterTechNodeAddedOrRemovedHandler(ICharacter character, TechNode techNode, bool isAdded)
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