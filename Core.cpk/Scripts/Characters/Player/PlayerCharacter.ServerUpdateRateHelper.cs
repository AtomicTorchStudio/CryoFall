namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public partial class PlayerCharacter
    {
        private class Bootstrapper : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                Server.Characters.PlayerOnlineStateChanged += PlayerOnlineStateChangedHandler;
            }

            private static void PlayerOnlineStateChangedHandler(ICharacter character, bool isOnline)
            {
                character.ProtoGameObject.ServerSetUpdateRate(character, isRare: !isOnline);
            }
        }
    }
}