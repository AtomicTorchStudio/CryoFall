namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using AtomicTorch.CBND.CoreMod.Systems.Achievements;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public static class PlayerCharacterAchievementsExtensions
    {
        public static PlayerCharacterAchievements SharedGetAchievements(this ICharacter character)
        {
            return PlayerCharacter.GetPrivateState(character)
                                  .Achievements;
        }
    }
}