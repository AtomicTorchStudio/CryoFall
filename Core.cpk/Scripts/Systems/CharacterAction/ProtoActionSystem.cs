namespace AtomicTorch.CBND.CoreMod.Systems
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public abstract class ProtoActionSystem<TSystem, TActionRequest, TActionState, TPublicActionState>
        : ProtoSystem<TSystem>
        where TSystem : ProtoActionSystem<TSystem, TActionRequest, TActionState, TPublicActionState>, new()
        where TActionRequest : IActionRequest
        where TActionState : ActionSystemState<TSystem, TActionRequest, TActionState, TPublicActionState>
        where TPublicActionState : BasePublicActionState, new()
    {
        public bool ClientTryAbortAction()
        {
            var privateState = ClientCurrentCharacterHelper.PrivateState;
            if (privateState.CurrentActionState is TActionState currentActionState)
            {
                // cancel state
                this.SharedAbortAction(currentActionState);
                return true;
            }

            // no such state - cannot cancel
            return false;
        }

        public void ClientTryStartAction()
        {
            var request = this.ClientTryCreateRequest(Client.Characters.CurrentPlayerCharacter);
            this.SharedStartAction(request);
        }

        public void SharedAbortAction(TActionState state)
        {
            this.SharedAbortActionInternal(state.Character, state.Request);
        }

        public TActionState SharedGetCurrentActionState(ICharacter character)
        {
            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            return characterPrivateState.CurrentActionState as TActionState;
        }

        public void SharedOnActionCompleted(TActionState state)
        {
            var character = state.Character;
            var request = state.Request;
            Logger.Info("Action completed: " + request, character);

            if (state.TargetWorldObject != null)
            {
                InteractionCheckerSystem.SharedUnregister(character, state.TargetWorldObject, isAbort: false);
            }

            var canComplete = true;
            try
            {
                // ensure the request is still valid
                this.SharedValidateRequest(request);
            }
            catch (Exception ex)
            {
                canComplete = false;
                Logger.Warning("Exception during completed action processing: "
                               + ex.Message
                               + Environment.NewLine
                               + request);
            }

            if (canComplete)
            {
                try
                {
                    this.SharedOnActionCompletedInternal(state, character);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex, "Exception during completed action processing");
                }
            }

            PlayerCharacter.GetPrivateState(character)
                           .SetCurrentActionState(null);
        }

        public bool SharedStartAction(TActionRequest request)
        {
            if (request == null)
            {
                return false;
            }

            var character = request.Character;

            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            if (characterPrivateState.CurrentActionState is TActionState existingState
                && this.SharedIsSameAction(existingState, request))
            {
                // the same action is already in process
                Logger.Info(
                    $"Action cannot be started: {request} - already performing the same action",
                    character);
                return false;
            }

            try
            {
                this.SharedValidateRequest(request);
            }
            catch (Exception ex)
            {
                Logger.Info(
                    $"Action cannot be started: {request} the request is not valid: {Environment.NewLine}{ex.Message}",
                    character);
                return false;
            }

            var state = this.SharedTryCreateState(request);
            if (state == null)
            {
                Logger.Info(
                    "Action cannot be started: " + request,
                    character);
                return false;
            }

            state.Request = request;
            InteractionCheckerSystem.CancelCurrentInteraction(character);
            characterPrivateState.SetCurrentActionState(state);

            Logger.Info("Action started: " + request, character);

            if (IsClient)
            {
                this.ClientOnStartActionCompleted(request, state);
            }

            if (state.TargetWorldObject != null)
            {
                InteractionCheckerSystem.SharedRegister(
                    character,
                    state.TargetWorldObject,
                    finishAction: isAbort =>
                                  {
                                      if (!isAbort)
                                      {
                                          return;
                                      }

                                      Logger.Warning(
                                          $"InteractionCheckerSystem interaction check failed - cancelling \"{this.ShortId}\" action: {state}",
                                          character);
                                      this.SharedAbortActionInternal(character, state.Request);
                                  });
            }

            return true;
        }

        protected abstract TActionRequest ClientTryCreateRequest(ICharacter character);

        protected abstract void SharedOnActionCompletedInternal(TActionState state, ICharacter character);

        protected abstract TActionState SharedTryCreateState(TActionRequest request);

        protected abstract void SharedValidateRequest(TActionRequest request);

        private async void ClientOnStartActionCompleted(TActionRequest request, TActionState state)
        {
            var isSuccess = await this.CallServer(_ => _.ServerRemote_StartAction(request));
            if (isSuccess)
            {
                return;
            }

            if (state.IsCancelled
                || state.IsCompleted)
            {
                return;
            }

            Logger.Info("Server refused to start action: " + request + ", cancelling it on the client");
            state.IsCancelledByServer = true;
            state.Cancel();
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_AbortAction(TActionRequest request)
        {
            this.SharedAbortActionInternal(ClientCurrentCharacterHelper.Character,
                                           request);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ServerRemote_AbortAction(TActionRequest request)
        {
            this.SharedAbortActionInternal(ServerRemoteContext.Character, request);
        }

        // TODO: should we use a custom remote call settings here? But it's an RPC with the result so it's not possible...
        private bool ServerRemote_StartAction(TActionRequest request)
        {
            request.Character = ServerRemoteContext.Character;
            return this.SharedStartAction(request);
        }

        private void SharedAbortActionInternal(ICharacter character, TActionRequest request)
        {
            var characterPrivateState = PlayerCharacter.GetPrivateState(character);
            var state = characterPrivateState.CurrentActionState;
            if (!(state is TActionState actionState))
            {
                // no action or action of another state type
                return;
            }

            if (!request.Equals(actionState.Request))
            {
                // different request active
                return;
            }

            if (!state.IsCompleted)
            {
                // reset action state
                actionState.Cancel();
                // return now - because this method was just called again by SetCurrentActionState
                return;
            }

            // ensure the action state is reset
            characterPrivateState.SetCurrentActionState(null);

            Logger.Info("Action cancelled: " + state, character);

            if (state.TargetWorldObject != null)
            {
                InteractionCheckerSystem.SharedUnregister(character, state.TargetWorldObject, isAbort: false);
            }

            if (IsClient
                && !state.IsCancelledByServer)
            {
                Instance.CallServer(_ => _.ServerRemote_AbortAction(request));
            }
            else if (IsServer
                     && !ServerRemoteContext.IsRemoteCall)
            {
                Instance.CallClient(character, _ => _.ClientRemote_AbortAction(request));
                // TODO: notify other players as well
            }
        }

        private bool SharedIsSameAction(TActionState existingState, TActionRequest request)
        {
            // ensure that the state type is exactly matching
            return existingState.GetType() == typeof(TActionState)
                   // ensure that the state was created from the same request
                   && existingState.Request.Equals(request);
        }
    }
}