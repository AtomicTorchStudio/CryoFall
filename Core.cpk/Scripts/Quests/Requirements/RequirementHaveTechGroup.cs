namespace AtomicTorch.CBND.CoreMod.Quests
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementHaveTechGroup : QuestRequirementWithDefaultState
    {
        public const string DescriptionFormat = "Unlock technology group: {0}";

        private RequirementHaveTechGroup(TechGroup techGroup)
            : base(description: null)
        {
            this.TechGroup = techGroup;
        }

        public override bool IsReversible => true;

        public TechGroup TechGroup { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat, this.TechGroup.Name);

        public static RequirementHaveTechGroup Require<TTechGroup>()
            where TTechGroup : TechGroup, new()
        {
            var proto = Api.GetProtoEntity<TTechGroup>();
            return new RequirementHaveTechGroup(proto);
        }

        protected override bool ServerIsSatisfied(ICharacter character, QuestRequirementState state)
        {
            return character.SharedGetTechnologies()
                            .SharedIsGroupUnlocked(this.TechGroup);
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                PlayerCharacterTechnologies.CharacterGroupAddedOrRemoved += this.CharacterGroupAddedOrRemovedHandler;
            }
            else
            {
                PlayerCharacterTechnologies.CharacterGroupAddedOrRemoved -= this.CharacterGroupAddedOrRemovedHandler;
            }
        }

        private void CharacterGroupAddedOrRemovedHandler(ICharacter character, TechGroup techGroup, bool isAdded)
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