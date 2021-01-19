namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Ruins.Gates
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectRuinsButton : ProtoObjectMisc
    {
        public abstract byte GateSearchRadius { get; }

        public override string InteractionTooltipText => InteractionTooltipTexts.Press;

        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        public abstract double OpenedDuration { get; }

        protected override void ClientInteractFinish(ClientObjectData data)
        {
            this.CallServer(_ => _.ServerRemote_OpenGate(data.GameObject));
            this.SoundPresetObject.PlaySound(ObjectSound.InteractSuccess);
        }

        protected override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();
            Api.Assert(this.OpenedDuration > 0,   nameof(this.OpenedDuration) + " must be > 0");
            Api.Assert(this.GateSearchRadius > 0, nameof(this.GateSearchRadius) + " must be > 0");
        }

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            return base.PrepareSoundPresetObject()
                       .Clone().Replace(ObjectSound.InteractSuccess,
                                        "Objects/Misc/ObjectGateRuinsButton/Use");
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        private void ClientRemote_ButtonPressedByOtherCharacter(Vector2Ushort objectButtonTilePosition)
        {
            this.SoundPresetObject.PlaySound(ObjectSound.InteractSuccess,
                                             worldPosition: objectButtonTilePosition.ToVector2D() + this.Layout.Center);
        }

        private void ServerRemote_OpenGate(IStaticWorldObject objectButton)
        {
            var character = ServerRemoteContext.Character;
            if (!this.SharedCanInteract(character, objectButton, writeToLog: true))
            {
                return;
            }

            // find the nearest gate
            var position = objectButton.TilePosition;
            var gatesNearby = Server.World.GetStaticWorldObjectsOfProtoInBounds<ObjectGateRuins>(
                new RectangleInt(position.X, position.Y, 1, 1)
                    .Inflate(this.GateSearchRadius));

            var objectGate = gatesNearby.OrderBy(g => g.TilePosition.TileDistanceTo(position))
                                        .FirstOrDefault();

            if (objectGate is null)
            {
                Logger.Error("No gate nearby found for " + objectButton);
                return;
            }

            // open the nearest gate
            var gatePrivateState = ObjectGateRuins.GetPrivateState(objectGate);
            gatePrivateState.OpenedUntil = Server.Game.FrameTime + this.OpenedDuration;

            using var tempListPlayersNearby = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(objectButton, tempListPlayersNearby.AsList());

            tempListPlayersNearby.Remove(character);
            if (tempListPlayersNearby.Count == 0)
            {
                return;
            }

            this.CallClient(tempListPlayersNearby.AsList(),
                            _ => _.ClientRemote_ButtonPressedByOtherCharacter(objectButton.TilePosition));
        }
    }
}