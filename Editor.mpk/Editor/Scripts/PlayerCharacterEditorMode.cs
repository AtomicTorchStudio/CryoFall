namespace AtomicTorch.CBND.CoreMod.Editor.Scripts
{
    using System;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects;
    using AtomicTorch.CBND.CoreMod.Editor.Data;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    [RemoteAuthorizeServerOperator]
    public class PlayerCharacterEditorMode : PlayerCharacter
    {
        public override float CharacterWorldHeight => 0;

        public override string Name => "Player character in Editor mode";

        public override void ClientDeinitialize(ICharacter character)
        {
            if (character.IsCurrentClientCharacter)
            {
                // reset
                BootstrapperClientGame.Init(null);
                EditorActiveToolManager.Deactivate();
            }
        }

        public override void ClientOnServerPhysicsUpdate(
            IWorldObject worldObject,
            Vector2D serverPosition,
            bool forceReset)
        {
            if (forceReset)
            {
                base.ClientOnServerPhysicsUpdate(worldObject, serverPosition, forceReset: true);
            }
        }

        public void ServerRemote_SwitchToEditorMode()
        {
            var character = ServerRemoteContext.Character;
            // the order of calls is important here
            Server.Characters.SetSpectatorMode(character, isSpectator: true, reinitilize: false);
            Server.Characters.SetProto(character, GetProtoEntity<PlayerCharacterEditorMode>());
            character.ServerRemoveAllStatusEffects();
        }

        public void ServerRemote_SwitchToPlayerMode()
        {
            var character = ServerRemoteContext.Character;
            if (character.ProtoCharacter != this)
            {
                throw new Exception("Incorrect proto");
            }

            // the order of calls is important here
            Server.Characters.SetSpectatorMode(character, isSpectator: false, reinitilize: false);
            Server.Characters.SetProto(character, GetProtoEntity<PlayerCharacter>());
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            var character = data.GameObject;
            if (!character.IsCurrentClientCharacter)
            {
                return;
            }

            BootstrapperClientGame.Init(character);
            Client.Scene.GetSceneObject(character)
                  .AddComponent<ComponentPlayerInputUpdater>()
                  .Setup(character);

            ClientPostEffectsManager.IsPostEffectsEnabled = false;
            //ClientTileBlendHelper.IsBlendingEnabled = false;
            ClientTileDecalHelper.IsDecalsEnabled = false;
        }

        protected override void ClientUpdate(ClientUpdateData data)
        {
            var character = data.GameObject;
            if (!character.IsCurrentClientCharacter)
            {
                return;
            }

            this.ClientUpdateMoveSpeed(character);
            this.SharedApplyInput(character, data.SyncPrivateState);
            this.CallServer(_ => _.ServerRemote_SetPosition(character.Position));
        }

        protected override void ServerPrepareCharacter(ServerInitializeData data)
        {
            base.ServerPrepareCharacter(data);
            // ensure the character is in spectator mode
            Server.Characters.SetSpectatorMode(data.GameObject, isSpectator: true, reinitilize: false);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            //var character = data.GameObject;
            //this.SharedApplyInput(character, data.PrivateState);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            // do not create any physics for spectator characters
            //base.CreatePhysics(data);
        }

        protected override void SharedGetSkeletonProto(
            ICharacter character,
            out ProtoCharacterSkeleton protoSkeleton,
            ref double scale)
        {
            // the skeleton in Editor mode will not be used, but it must be provided to avoid exception
            protoSkeleton = GetProtoEntity<SkeletonHumanMale>();
        }

        private void ClientUpdateMoveSpeed(ICharacter character)
        {
            var inputService = Api.Client.Input;
            var isShiftKeyHeld = inputService.IsKeyHeld(InputKey.Shift,  evenIfHandled: true);
            var isCtrlKeyHeld = inputService.IsKeyHeld(InputKey.Control, evenIfHandled: true);

            var speed = 5.0;
            if (isShiftKeyHeld)
            {
                speed *= 3.0;
            }

            if (isCtrlKeyHeld)
            {
                speed *= 1 / 3.0;
            }

            // multiple speed on the reversed camera zoom factor
            speed *= 1.0 / Math.Sqrt(Client.Rendering.WorldCameraCurrentZoom);

            if (Api.Client.Characters.GetCurrentCharacterMoveSpeed() == speed)
            {
                return;
            }

            Api.Client.Characters.SetCurrentCharacterMoveSpeed(speed);
            this.CallServer(_ => _.ServerRemote_SetSpeed(speed));
        }

        [RemoteCallSettings(DeliveryMode.UnreliableSequenced, maxCallsPerSecond: 10)]
        private void ServerRemote_SetPosition(Vector2D position)
        {
            var character = ServerRemoteContext.Character;
            if (character.ProtoCharacter != this)
            {
                Logger.Warning(this.Id + " - cannot set position for character. Different proto: " + character);
                return;
            }

            Server.World.SetPosition(character, position, writeToLog: false);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, maxCallsPerSecond: 60)]
        private void ServerRemote_SetSpeed(double speed)
        {
            var character = ServerRemoteContext.Character;
            if (character.ProtoCharacter != this)
            {
                return;
            }

            Server.Characters.SetMoveSpeed(character, speed);
        }

        private void SharedApplyInput(ICharacter character, PlayerCharacterPrivateState privateState)
        {
            var input = privateState.Input;

            // apply input
            privateState.Input.SetChanged(false);

            var moveModes = input.MoveModes;

            double directionX = 0, directionY = 0;
            if ((moveModes & CharacterMoveModes.Up) != 0)
            {
                directionY = 1;
            }

            if ((moveModes & CharacterMoveModes.Down) != 0)
            {
                directionY = -1;
            }

            if ((moveModes & CharacterMoveModes.Left) != 0)
            {
                directionX = -1;
            }

            if ((moveModes & CharacterMoveModes.Right) != 0)
            {
                directionX = 1;
            }

            Vector2D directionVector = (directionX, directionY);

            var moveSpeed = IsServer
                                ? Server.Characters.GetMoveSpeed(character)
                                : Client.Characters.GetCurrentCharacterMoveSpeed();

            var moveVelocity = directionVector.Normalized * moveSpeed;

            if (IsServer)
            {
                Server.Characters.SetVelocity(character, moveVelocity);
            }
            else
            {
                Client.Characters.SetVelocity(character, moveVelocity);
            }
        }
    }
}