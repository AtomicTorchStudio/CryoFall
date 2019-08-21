namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public static class PlayerCharacterFinalCacheExtensions
    {
        public static void SharedRefreshFinalCacheIfNecessary(this ICharacter character)
        {
            var publicState = character.GetPublicState<ICharacterPublicState>();
            var privateState = character.GetPrivateState<BaseCharacterPrivateState>();

            SharedCharacterStatsHelper.RefreshCharacterFinalStatsCache(
                ((IProtoCharacterCore)character.ProtoCharacter).ProtoCharacterDefaultEffects,
                publicState,
                privateState);
        }

        public static void SharedSetFinalStatsCacheDirty(this ICharacter character)
        {
            if (!character.IsInitialized)
            {
                return;
            }

            character.GetPrivateState<BaseCharacterPrivateState>()
                     .SetFinalStatsCacheIsDirty();
        }
    }
}