namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public abstract class BaseTechGroupRequirement
    {
        public abstract BaseViewModelTechGroupRequirement CreateViewModel();

        public bool IsSatisfied(ICharacter character, out string error)
        {
            return this.IsSatisfied(new CharacterContext(character), out error);
        }

        protected abstract bool IsSatisfied(CharacterContext context, out string error);

        protected struct CharacterContext
        {
            private PlayerCharacterTechnologies technologies;

            public CharacterContext(ICharacter playerCharacter)
            {
                this.Character = playerCharacter;
                this.technologies = null;
            }

            public ICharacter Character { get; }

            public PlayerCharacterTechnologies Technologies
                => this.technologies
                   ?? (this.technologies = this.Character.GetPrivateState<PlayerCharacterPrivateState>()
                                               .Technologies);
        }
    }
}