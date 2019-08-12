namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientCurrentCharacterFinalStatsHelper
    {
        private static ICharacter character;

        private static PlayerCharacterPrivateState privateState;

        private static IStateSubscriptionOwner subscriptionStorage;

        public static event Action FinalStatsCacheChanged;

        public static FinalStatsCache FinalStatsCache => privateState.FinalStatsCache;

        public static void Init(ICharacter character)
        {
            ClientCurrentCharacterFinalStatsHelper.character = ClientCurrentCharacterHelper.Character;
            privateState = PlayerCharacter.GetPrivateState(character);

            subscriptionStorage?.Dispose();
            subscriptionStorage = new StateSubscriptionStorage();
            privateState.ClientSubscribe(_ => _.FinalStatsCache,
                                         _ => Api.SafeInvoke(FinalStatsCacheChanged),
                                         subscriptionStorage);

            Api.SafeInvoke(FinalStatsCacheChanged);
        }
    }
}