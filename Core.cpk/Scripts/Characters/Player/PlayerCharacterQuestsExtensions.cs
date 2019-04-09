namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public static class PlayerCharacterQuestsExtensions
    {
        public static PlayerCharacterQuests SharedGetQuests(this ICharacter character)
        {
            return PlayerCharacter.GetPrivateState(character)
                                  .Quests;
        }
    }
}