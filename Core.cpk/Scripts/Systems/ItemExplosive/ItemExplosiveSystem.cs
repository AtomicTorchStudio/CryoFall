namespace AtomicTorch.CBND.CoreMod.Systems.ItemExplosive
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Explosives;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ItemExplosiveSystem
        : ProtoActionSystem<
            ItemExplosiveSystem,
            ItemExplosiveRequest,
            ItemExplosiveActionState,
            ItemExplosiveActionState.PublicState>
    {
        public override string Name => "Explosive Items System";

        public void ClientTryStartAction(Vector2Ushort targetPosition)
        {
            var request = this.ClientTryCreateRequest(Client.Characters.CurrentPlayerCharacter, targetPosition);
            this.SharedStartAction(request);
        }

        protected override ItemExplosiveRequest ClientTryCreateRequest(ICharacter character)
        {
            throw new NotImplementedException("Please use the override of this method");
        }

        protected override void SharedOnActionCompletedInternal(ItemExplosiveActionState state, ICharacter character)
        {
            if (IsClient)
            {
                return;
            }

            var protoItemExplosive = (IProtoItemExplosive)state.ItemExplosive.ProtoItem;
            protoItemExplosive.ServerOnUseActionFinished(character, state.ItemExplosive, state.TargetPosition);
        }

        protected override ItemExplosiveActionState SharedTryCreateState(ItemExplosiveRequest request)
        {
            var item = request.Item;
            var durationSeconds = ((IProtoItemExplosive)item.ProtoItem).DeployDuration.TotalSeconds;

            // apply stat for planting speed
            durationSeconds *= request.Character.SharedGetFinalStatMultiplier(
                StatName.ItemExplosivePlantingSpeedMultiplier);

            if (IsClient)
            {
                // add ping duration - because this action result happens only on the Server-side
                durationSeconds += Client.CurrentGame.PingGameSeconds;
            }

            return new ItemExplosiveActionState(request.Character,
                                                request.TargetPosition,
                                                durationSeconds,
                                                item);
        }

        protected override void SharedValidateRequest(ItemExplosiveRequest request)
        {
            if (IsServer
                && !Server.Core.IsInPrivateScope(request.Character, request.Item))
            {
                throw new Exception(
                    $"{request.Character} cannot access {request.Item} because it's container is not in the private scope");
            }

            if (!(request.Item.ProtoItem is IProtoItemExplosive protoItemExplosive))
            {
                throw new Exception("The item must be an explosive");
            }

            if (!protoItemExplosive.SharedValidatePlacement(request.Character,
                                                            request.TargetPosition,
                                                            logErrors: true))
            {
                throw new Exception("The explosive item deploy distance exceeded");
            }
        }

        private ItemExplosiveRequest ClientTryCreateRequest(ICharacter character, Vector2Ushort targetPosition)
        {
            var item = character.SharedGetPlayerSelectedHotbarItem();
            if (!(item.ProtoItem is IProtoItemExplosive protoItemExplosive))
            {
                // no explosive item selected
                return null;
            }

            if (!protoItemExplosive.SharedValidatePlacement(character,
                                                            targetPosition,
                                                            logErrors: true))
            {
                return null;
            }

            return new ItemExplosiveRequest(character, item, targetPosition);
        }
    }
}