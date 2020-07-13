namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects
{
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public readonly struct EffectActionContext
    {
        public readonly ICharacter Character;

        public EffectActionContext(ICharacter character)
        {
            this.Character = character;
        }
    }
}