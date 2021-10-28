namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public static class PlayerCharacterFinalCacheExtensions
    {
        public static void SharedSetFinalStatsCacheDirty(this ICharacter character)
        {
            if (character is null)
            {
                return;
            }

            if (!character.IsInitialized)
            {
                return;
            }

            character.GetPrivateState<BaseCharacterPrivateState>()
                     .SetFinalStatsCacheIsDirty();
        }
    }
}