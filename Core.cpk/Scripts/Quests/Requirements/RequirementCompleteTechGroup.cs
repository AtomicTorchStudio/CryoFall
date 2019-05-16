namespace AtomicTorch.CBND.CoreMod.Quests
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementCompleteTechGroup : QuestRequirementWithDefaultState
    {
        public const string DescriptionFormat = "Complete: {0} (tech group)";

        private RequirementCompleteTechGroup(TechGroup techGroup)
            : base(description: null)
        {
            this.TechGroup = techGroup;
        }

        public override bool IsReversible => true;

        public TechGroup TechGroup { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.TechGroup.Name);

        public static RequirementCompleteTechGroup Require<TTechGroup>()
            where TTechGroup : TechGroup, new()
        {
            var proto = Api.GetProtoEntity<TTechGroup>();
            return new RequirementCompleteTechGroup(proto);
        }

        protected override bool ServerIsSatisfied(ICharacter character, QuestRequirementState state)
        {
            var completedCount = 0;
            var techNodes = character.SharedGetTechnologies().Nodes;
            foreach (var techNode in techNodes)
            {
                if (techNode.Group == this.TechGroup)
                {
                    completedCount++;
                }
            }

            return completedCount >= this.TechGroup.Nodes.Count;
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
            if (techNode.Group != this.TechGroup)
            {
                return;
            }

            var context = this.GetActiveContext(character, out _);
            context?.Refresh();
        }
    }
}