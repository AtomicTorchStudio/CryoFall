namespace AtomicTorch.CBND.CoreMod.Editor
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.WorldDiscovery;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// This system will discover the world map for Editor's user.
    /// </summary>
    public class ServerEditorWorldMapSystem : ProtoSystem<ServerEditorWorldMapSystem>
    {
        public override string Name => nameof(ServerEditorWorldMapSystem);

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            Server.Characters.PlayerOnlineStateChanged += PlayerOnlineStateChangedHandler;
        }

        private static void PlayerOnlineStateChangedHandler(ICharacter playerCharacter, bool isOnline)
        {
            if (!isOnline)
            {
                return;
            }

            var key = "IsMapDiscovered_" + playerCharacter.Name;
            if (Api.Server.Database.TryGet("Core", key, out bool isMapDiscovered)
                && isMapDiscovered)
            {
                return;
            }

            Api.Server.Database.Set("Core", key, true);
            WorldDiscoverySystem.Instance.ServerDiscoverWorldChunks(
                playerCharacter,
                new List<Vector2Ushort>(Server.World.GetAllChunkTilePositions()));
        }
    }
}