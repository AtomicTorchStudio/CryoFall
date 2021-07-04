namespace AtomicTorch.CBND.CoreMod.Helpers
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    [NotPersistent]
    public class SharedLocalServerHelper : ProtoSystem<SharedLocalServerHelper>
    {
        static SharedLocalServerHelper()
        {
            if (IsServer)
            {
                IsLocalServer = Api.Server.Core.GetLaunchArgumentValue("+localServer") == "1";
            }
        }

        public static event Action IsLocalServerPropertyChanged;

        /// <summary>
        /// Determines whether the connected server is a local server
        /// (may affect certain mechanics; e.g. there is no preparation timer for the boss on local server even in PvE).
        /// </summary>
        public static bool IsLocalServer { get; private set; }

        public override string Name => nameof(SharedLocalServerHelper);

        protected override void PrepareSystem()
        {
            if (IsServer)
            {
                Server.Characters.PlayerOnlineStateChanged += this.ServerPlayerOnlineStateChangedHandler;
            }
        }

        private void ClientRemote_SetIsLocalServer(bool isLocalServer)
        {
            IsLocalServer = isLocalServer;
            Api.SafeInvoke(IsLocalServerPropertyChanged);
        }

        private void ServerPlayerOnlineStateChangedHandler(ICharacter character, bool isOnline)
        {
            if (isOnline)
            {
                this.CallClient(character,
                                _ => this.ClientRemote_SetIsLocalServer(IsLocalServer));
            }
        }
    }
}