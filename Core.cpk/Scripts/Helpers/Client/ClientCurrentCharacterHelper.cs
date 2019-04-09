namespace AtomicTorch.CBND.CoreMod.Helpers.Client
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public static class ClientCurrentCharacterHelper
    {
        private static ICharacter character;

        public static ICharacter Character
        {
            get => character;
            //?? throw new Exception("Current player character is not assigned yet");
        }

        public static PlayerCharacterPrivateState PrivateState { get; private set; }

        public static PlayerCharacterPublicState PublicState { get; private set; }

        public static void Init(ICharacter character)
        {
            ClientCurrentCharacterHelper.character = character;
            PrivateState = PlayerCharacter.GetPrivateState(Character);
            PublicState = PlayerCharacter.GetPublicState(Character);
        }
    }
}