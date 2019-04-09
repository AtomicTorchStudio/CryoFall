namespace AtomicTorch.CBND.CoreMod.StaticObjects
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class InteractableStaticWorldObjectHelper : ProtoEntity
    {
        private static InteractableStaticWorldObjectHelper instance;

        private static int lastRequestId;

        private bool isAwaitingServerInteraction;

        public override string Name => nameof(InteractableStaticWorldObjectHelper);

        public static void ClientStartInteract(IStaticWorldObject worldObject)
        {
            instance.ClientInteractStartAsync(worldObject);
        }

        public static void ServerTryAbortInteraction(ICharacter character, IStaticWorldObject worldObject)
        {
            InteractionCheckerSystem.Unregister(character, worldObject, isAbort: true);
        }

        protected override void PrepareProto()
        {
            base.PrepareProto();
            instance = this;
        }

        private static IInteractableProtoStaticWorldObject SharedGetProto(IStaticWorldObject worldObject)
        {
            return (IInteractableProtoStaticWorldObject)worldObject.ProtoStaticWorldObject;
        }

        private async void ClientInteractStartAsync(IStaticWorldObject worldObject)
        {
            if (this.isAwaitingServerInteraction)
            {
                return;
            }

            var character = Client.Characters.CurrentPlayerCharacter;
            if (InteractionCheckerSystem.GetCurrentInteraction(character) == worldObject)
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
            if (menuWindow == null)
            {
                Logger.Important("Cannot open menu for object interaction with " + worldObject);
                this.CallServer(_ => _.ServerRemote_OnClientInteractFinish(worldObject));
                return;
            }

            ClientCurrentInteractionMenu.RegisterMenuWindow(menuWindow);

            InteractionCheckerSystem.Register(
                character,
                worldObject,
                finishAction: _ => menuWindow.CloseWindow());

            ClientInteractionUISystem.Register(
                worldObject,
                menuWindow,
                onMenuClosedByClient:
                () =>
                {
                    InteractionCheckerSystem.Unregister(character, worldObject, isAbort: false);
                    if (!worldObject.IsDestroyed)
                    {
                        ++lastRequestId;
                        this.CallServer(_ => _.ServerRemote_OnClientInteractFinish(worldObject));
                    }
                });

            Logger.Important("Started object interaction with " + worldObject);
            ClientCurrentInteractionMenu.Open();
        }

        private void ClientRemote_FinishInteraction(IStaticWorldObject worldObject)
        {
            Logger.Important($"Server informed that the object interaction with {worldObject} is finished");
            ClientInteractionUISystem.OnServerForceFinishInteraction(worldObject);
        }

        private void ServerFinishInteractionInternal(ICharacter who, IStaticWorldObject worldObject)
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
                    "Exception when calling " + nameof(IInteractableProtoStaticWorldObject.ServerOnMenuClosed));
            }

            Logger.Important($"Finished object interaction with {worldObject} for {who}");
        }

        private void ServerRemote_OnClientInteractFinish(IStaticWorldObject worldObject)
        {
            var character = ServerRemoteContext.Character;
            if (!InteractionCheckerSystem.Unregister(character, worldObject, isAbort: false))
            {
                return;
            }

            Logger.Important($"Client {character} informed that the object interaction with {worldObject} is finished");
            this.ServerFinishInteractionInternal(character, worldObject);
        }

        private bool ServerRemote_OnClientInteractStart(IStaticWorldObject worldObject)
        {
            var character = ServerRemoteContext.Character;

            if (worldObject == null
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
            InteractionCheckerSystem.Register(
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

            Logger.Important($"Started object interaction with {worldObject} for {character}");
            return true;
        }
    }
}