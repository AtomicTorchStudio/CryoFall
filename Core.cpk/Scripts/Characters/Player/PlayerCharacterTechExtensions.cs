namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public static class PlayerCharacterTechExtensions
    {
        public static PlayerCharacterTechnologies SharedGetTechnologies(this ICharacter character)
        {
            return PlayerCharacter.GetPrivateState(character)
                                  .Technologies;
        }
    }
}