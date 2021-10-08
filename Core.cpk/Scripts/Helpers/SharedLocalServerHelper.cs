namespace AtomicTorch.CBND.CoreMod.Helpers
{
    using System;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    [NotPersistent]
    public class SharedLocalServerHelper : ProtoSystem<SharedLocalServerHelper>
    {
        private static TaskCompletionSource<bool> ClientTaskIsLocalServerPropertyReceivedCompletionSource;

        static SharedLocalServerHelper()
        {
            if (IsServer)
            {
                IsLocalServer = Api.Server.Core.IsLocalServer;
            }
        }

        public static event Action IsLocalServerPropertyChanged;

        public static Task ClientTaskIsLocalServerPropertyReceived
            => ClientTaskIsLocalServerPropertyReceivedCompletionSource.Task;

        /// <summary>
        /// Determines whether the connected server is a local server
        /// (may affect certain mechanics).
        /// </summary>
        public static bool IsLocalServer { get; private set; }

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
            ClientTaskIsLocalServerPropertyReceivedCompletionSource.TrySetResult(true);
        }

        private void ServerPlayerOnlineStateChangedHandler(ICharacter character, bool isOnline)
        {
            if (isOnline)
            {
                this.CallClient(character,
                                _ => this.ClientRemote_SetIsLocalServer(IsLocalServer));
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += Reset;
                Reset();

                static void Reset()
                {
                    IsLocalServer = false;
                    ClientTaskIsLocalServerPropertyReceivedCompletionSource = new TaskCompletionSource<bool>();
                }
            }
        }
    }
}