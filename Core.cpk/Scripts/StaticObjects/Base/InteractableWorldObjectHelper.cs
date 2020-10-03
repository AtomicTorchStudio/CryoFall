namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class InteractableWorldObjectHelper : ProtoEntity
    {
        private static InteractableWorldObjectHelper instance;

        private static int lastRequestId;

        private bool isAwaitingServerInteraction;

        public delegate void DelegateClientMenuCreated(
            IWorldObject worldObject,
            BaseUserControlWithWindow menu);

        public static event DelegateClientMenuCreated ClientMenuCreated;

        public override string Name => nameof(InteractableWorldObjectHelper);

        public static void ClientStartInteract(IWorldObject worldObject)
        {
            instance.ClientInteractStartAsync(worldObject);
        }

        public static void ServerTryAbortInteraction(ICharacter character, IWorldObject worldObject)
        {
            InteractionCheckerSystem.SharedUnregister(character, worldObject, isAbort: true);
        }

        protected override void PrepareProto()
        {
            base.PrepareProto();
            instance = this;
        }

        private static IInteractableProtoWorldObject SharedGetProto(IWorldObject worldObject)
        {
            return (IInteractableProtoWorldObject)worldObject.ProtoGameObject;
        }

        private async void ClientInteractStartAsync(IWorldObject worldObject)
        {
            if (this.isAwaitingServerInteraction)
            {
                return;
            }

            var character = Client.Characters.CurrentPlayerCharacter;
            if (InteractionCheckerSystem.SharedGetCurrentInteraction(character) == worldObject)
            {
                // already interacting with this object
                return;
            }

            this.isAwaitingServerInteraction = true;
            try
            {
                var requestId = ++lastRequestId;
                var isOpened = await this.CallServer(_ => _.ServerRemote_OnClientInteractStart(worldObject));
                if (!isOpened
                    || requestId != lastRequestId)
                {
                    return;
                }
            }
            finally
            {
                this.isAwaitingServerInteraction = false;
            }

            var menuWindow = SharedGetProto(worldObject).ClientOpenUI(worldObject);
            if (menuWindow is null)
            {
                Logger.Info("Cannot open menu for object interaction with " + worldObject);
                this.CallServer(_ => _.ServerRemote_OnClientInteractFinish(worldObject));
                return;
            }

            Api.SafeInvoke(() => ClientMenuCreated?.Invoke(worldObject, menuWindow));

            ClientCurrentInteractionMenu.RegisterMenuWindow(menuWindow);

            InteractionCheckerSystem.SharedRegister(
                character,
                worldObject,
                finishAction: _ => menuWindow.CloseWindow());

            ClientInteractionUISystem.Register(
                worldObject,
                menuWindow,
                onMenuClosedByClient:
                () =>
                {
                    InteractionCheckerSystem.SharedUnregister(character, worldObject, isAbort: false);
                    if (!worldObject.IsDestroyed)
                    {
                        ++lastRequestId;
                        this.CallServer(_ => _.ServerRemote_OnClientInteractFinish(worldObject));
                    }
                });

            Logger.Info("Started object interaction with " + worldObject);
            ClientCurrentInteractionMenu.Open();
        }

        private void ClientRemote_FinishInteraction(IWorldObject worldObject)
        {
            Logger.Info($"Server informed that the object interaction with {worldObject} is finished");
            ClientInteractionUISystem.OnServerForceFinishInteraction(worldObject);
        }

        private void ServerFinishInteractionInternal(ICharacter who, IWorldObject worldObject)
        {
            if (worldObject.IsDestroyed)
            {
                return;
            }

            Server.World.ExitPrivateScope(who, worldObject);

            try
            {
                SharedGetProto(worldObject).ServerOnMenuClosed(who, worldObject);
            }
            catch (Exception ex)
            {
                Logger.Exception(
                    ex,
                    "Exception when calling " + nameof(IInteractableProtoWorldObject.ServerOnMenuClosed));
            }

            Logger.Info($"Finished object interaction with {worldObject} for {who}");
        }

        private void ServerRemote_OnClientInteractFinish(IWorldObject worldObject)
        {
            var character = ServerRemoteContext.Character;
            if (!InteractionCheckerSystem.SharedUnregister(character, worldObject, isAbort: false))
            {
                return;
            }

            Logger.Info($"Client {character} informed that the object interaction with {worldObject} is finished");
            this.ServerFinishInteractionInternal(character, worldObject);
        }

        private bool ServerRemote_OnClientInteractStart(IWorldObject worldObject)
        {
            var character = ServerRemoteContext.Character;

            if (worldObject is null
                || !worldObject.ProtoWorldObject.SharedCanInteract(character, worldObject, writeToLog: true))
            {
                // player is too far from the world object or world object is destroyed
                return false;
            }

            InteractionCheckerSystem.CancelCurrentInteraction(character);

            var proto = SharedGetProto(worldObject);

            proto.ServerOnClientInteract(character, worldObject);

            if (proto.IsAutoEnterPrivateScopeOnInteraction)
            {
                // enter private scope - containers will be sent to the player character
                Server.World.EnterPrivateScope(character, worldObject);
            }

            // register private scope exit on interaction cancel
            InteractionCheckerSystem.SharedRegister(
                character,
                worldObject,
                finishAction: isAbort =>
                              {
                                  if (worldObject.IsDestroyed)
                                  {
                                      return;
                                  }

                                  this.ServerFinishInteractionInternal(character, worldObject);

                                  if (isAbort)
                                  {
                                      // notify client
                                      this.CallClient(character, _ => this.ClientRemote_FinishInteraction(worldObject));
                                  }
                              });

            Logger.Info($"Started object interaction with {worldObject} for {character}");
            return true;
        }
    }
}