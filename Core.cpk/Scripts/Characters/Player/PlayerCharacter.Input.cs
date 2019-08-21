namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Input.ClientPrediction;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Melee;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    // partial class containing only input methods
    public partial class PlayerCharacter
    {
        /// <summary>
        /// Checks if current hotbar item is different
        /// </summary>
        public static void SharedRefreshSelectedHotbarItem(
            ICharacter character,
            PlayerCharacterPrivateState privateState)
        {
            if ((IsServer || character.IsCurrentClientCharacter)
                && privateState.ContainerHotbar.StateHash
                != privateState.ContainerHotbarLastStateHash)
            {
                // need to refresh the selected hotbar item
                SharedSelectHotbarSlotId(character, privateState.SelectedHotbarSlotId);
            }
        }

        public static void SharedSelectHotbarSlotId(ICharacter character, byte? slotId)
        {
            var privateState = GetPrivateState(character);
            var publicState = GetPublicState(character);
            var containerHotbar = privateState.ContainerHotbar;

            privateState.ContainerHotbarLastStateHash = containerHotbar.StateHash;
            privateState.SelectedHotbarSlotId = slotId;

            IItem item;
            if (slotId.HasValue)
            {
                item = containerHotbar.GetItemAtSlot(slotId.Value);

                if (item != null
                    && !item.ProtoItem.SharedCanSelect(item, character))
                {
                    item = null;
                    privateState.SelectedHotbarSlotId = slotId = null;
                }
            }
            else
            {
                item = null;
            }

            if (publicState.SelectedHotbarItem != item)
            {
                publicState.SetSelectedHotbarItem(item);
                //Logger.Info($"Selected hotbar item: slotId={slotId} item: {item?.ToString() ?? "<none>"}", character);
            }
            //else
            //{
            //    Logger.Info($"Hotbar item is already selected: slotId={slotId} item: {item?.ToString() ?? "<none>"}",
            //                character);
            //}

            // update selected weapon
            if (publicState.SelectedHotbarItem == null)
            {
                publicState.SetCurrentWeaponProtoOnly(ItemNoWeapon.Instance);
            }

            var isSelectedItemWeapon = item?.ProtoItem is IProtoItemWeapon;
            privateState.WeaponState.SharedSetWeaponItem(
                item: isSelectedItemWeapon ? item : null,
                protoItem: publicState.CurrentItemWeaponProto);
        }

        public void ClientSelectHotbarSlot(byte? slotId)
        {
            var character = Client.Characters.CurrentPlayerCharacter;
            if (GetPrivateState(character).SelectedHotbarSlotId == slotId)
            {
                return;
            }

            this.CallServer(_ => _.ServerRemote_SelectHotbarSlotId(slotId));
            SharedSelectHotbarSlotId(character, slotId);
        }

        public void ClientSetInput(CharacterInputUpdate data)
        {
            var character = Client.Characters.CurrentPlayerCharacter;
            if (!this.SharedApplyInputUpdate(data, character))
            {
                // input not changed
                return;
            }

            GetClientState(Client.Characters.CurrentPlayerCharacter)
                .ComponentPlayerInputSender.Send(data);
        }

        [RemoteCallSettings(DeliveryMode.UnreliableSequenced, maxCallsPerSecond: 120)]
        internal void ServerRemote_SetInput(CharacterInputUpdate data, byte inputId)
        {
            var character = ServerRemoteContext.Character;
            this.CallClient(character, _ => _.ClientRemote_ServerAckInput(inputId));

            var privateState = GetPrivateState(character);

            if (SequenceNumberHelper.GetRelativeSequenceNumberForByte(inputId, privateState.ServerLastAckClientInputId)
                <= 0)
            {
                //Logger.WriteDev("Server already received this or newer input: " + inputId);
                return;
            }

            //Logger.WriteDev("Server received new input: " + inputId + ": " + data);
            privateState.ServerLastAckClientInputId = inputId;
            this.SharedApplyInputUpdate(data, character);
        }

        protected void SharedApplyInput(
            ICharacter character,
            PlayerCharacterPrivateState privateState,
            PlayerCharacterPublicState publicState)
        {
            var characterIsOffline = !character.ServerIsOnline;
            if (characterIsOffline)
            {
                privateState.Input = default;
            }

            // please note - input is a structure so actually we're implicitly copying it here
            var input = privateState.Input;
            // please note - applied input is a class and we're getting it by reference here
            var appliedInput = publicState.AppliedInput;
            var hasRunningFlag = (input.MoveModes & CharacterMoveModes.ModifierRun) != 0;
            var isRunning = hasRunningFlag;

            if (isRunning)
            {
                var stats = publicState.CurrentStatsExtended;
                var wasRunning = (appliedInput.MoveModes & CharacterMoveModes.ModifierRun) != 0;
                var staminaCurrent = stats.StaminaCurrent;

                if (!wasRunning)
                {
                    // can start running only when the current energy is at least on 10%
                    isRunning = staminaCurrent >= 0.1 * stats.StaminaMax;
                }
                else // if was running
                {
                    // can continue to run while has energy
                    isRunning = staminaCurrent > 0;
                }

                //Logger.WriteDev($"Was running: {wasRunning}; now running: {isRunning}; stamina: {staminaCurrent}");
            }

            double moveSpeed;
            if (characterIsOffline
                || (privateState.CurrentActionState?.IsBlocksMovement ?? false))
            {
                // offline or current action blocks movement
                moveSpeed = 0;
                isRunning = false;
                input.MoveModes = CharacterMoveModes.None;
            }
            else
            {
                var characterFinalStateCache = privateState.FinalStatsCache;
                moveSpeed = characterFinalStateCache[StatName.MoveSpeed];
                moveSpeed *= ProtoTile.SharedGetTileMoveSpeedMultiplier(character.Tile);

                if (isRunning)
                {
                    var moveSpeedMultiplier = characterFinalStateCache[StatName.MoveSpeedRunMultiplier];
                    if (moveSpeedMultiplier > 0)
                    {
                        moveSpeed = moveSpeed * moveSpeedMultiplier;
                    }
                    else
                    {
                        isRunning = false;
                    }
                }
            }

            if (!isRunning)
            {
                // cannot run - remove running flag
                input.MoveModes &= ~CharacterMoveModes.ModifierRun;
            }

            if (appliedInput.MoveModes == input.MoveModes
                && appliedInput.RotationAngleRad == input.RotationAngleRad
                && publicState.AppliedInput.MoveSpeed == moveSpeed)
            {
                // input is not changed
                return;
            }

            // apply new input
            appliedInput.Set(input, moveSpeed);

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
            var moveVelocity = directionVector.Normalized * moveSpeed;

            if (IsServer)
            {
                Server.Characters.SetVelocity(character, moveVelocity);
            }
            else // if client
            {
                if (ClientCurrentCharacterLagPredictionManager.IsLagPredictionEnabled)
                {
                    Client.Characters.SetVelocity(character, moveVelocity);
                }
            }
        }

        protected bool SharedApplyInputUpdate(CharacterInputUpdate data, ICharacter character)
        {
            var privateState = GetPrivateState(character);
            var input = privateState.Input;
            input.MoveModes = data.MoveModes;
            input.RotationAngleRad = data.RotationAngleRad;
            if (input.Equals(privateState.Input))
            {
                // input not changed
                return false;
            }

            // input changed
            privateState.Input = input;
            return true;
        }

        [RemoteCallSettings(DeliveryMode.UnreliableSequenced, maxCallsPerSecond: 60, avoidBuffer: true)]
        private void ClientRemote_ServerAckInput(byte inputId)
        {
            var playerCharacter = Client.Characters.CurrentPlayerCharacter;
            if (playerCharacter != null)
            {
                var clientState = GetClientState(playerCharacter);
                clientState?.ComponentPlayerInputSender.OnServerAck(inputId);
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ServerRemote_ResetLastInputAck()
        {
            var character = ServerRemoteContext.Character;
            var privateState = GetPrivateState(character);
            privateState.ServerLastAckClientInputId = 0;
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, maxCallsPerSecond: 60)]
        private void ServerRemote_SelectHotbarSlotId(byte? slotId)
        {
            SharedSelectHotbarSlotId(ServerRemoteContext.Character, slotId);
        }
    }
}