namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SharedLootDropNotifyHelper : ProtoEntity
    {
        private static readonly SoundResource SoundResourceDropLoot = new("Events/DropLoot");

        private static SharedLootDropNotifyHelper instance;

        public override string Name => nameof(SharedLootDropNotifyHelper);

        public static void ServerOnLootDropped(IItemsContainer container)
        {
            var worldObject = container?.OwnerAsStaticObject;
            if (worldObject is null)
            {
                return;
            }

            using var scopedBy = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(worldObject, scopedBy);
            instance.CallClient(scopedBy.AsList(),
                                _ => _.ClientRemote_PlayLootDroppedSound(worldObject.TilePosition));
        }

        protected override void PrepareProto()
        {
            instance = this;
        }

        private void ClientRemote_PlayLootDroppedSound(Vector2Ushort tilePosition)
        {
            Client.Audio.PlayOneShot(SoundResourceDropLoot,
                                     (tilePosition.X + 0.5, tilePosition.Y + 0.5));
        }
    }
}