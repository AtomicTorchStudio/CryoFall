namespace AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class EditorTerrainCopyPasteHelper
    {
        private const string ObjectPlacementGuide
            = @"[br]Press ENTER to paste in the selected destination.
                [br]Press ESC or Ctrl+Z to cancel.";

        private static readonly IClientStorage ClientStorage;

        private static BufferEntry? lastBufferEntry;

        static EditorTerrainCopyPasteHelper()
        {
            if (!Api.IsClient)
            {
                return;
            }

            ClientStorage = Api.Client.Storage.GetStorage("Editor/" + nameof(EditorTerrainCopyPasteHelper));
            ClientStorage.RegisterType(typeof(Vector2Ushort));
            ClientStorage.RegisterType(typeof(TerrainEditingSystem.TileModifyRequest));
            ClientStorage.RegisterType(typeof(BufferEntry));

            TryLoadBufferEntry();
        }

        public static void ClientCopy(BoundsUshort bounds)
        {
            if (bounds.Size == default)
            {
                return;
            }

            var tilesToModify = new List<TerrainEditingSystem.TileModifyRequest>();
            var worldService = Api.Client.World;

            for (ushort x = 0; x < bounds.Size.X; x++)
            for (ushort y = 0; y < bounds.Size.Y; y++)
            {
                var tile = worldService.GetTile(x + bounds.Offset.X,
                                                y + bounds.Offset.Y);
                tilesToModify.Add(
                    new TerrainEditingSystem.TileModifyRequest(
                        new Vector2Ushort(x, y),
                        tile.ProtoTile.SessionIndex,
                        tile.Height,
                        tile.IsSlope));
            }

            lastBufferEntry = new BufferEntry(tilesToModify, bounds.Size);
            NotificationSystem.ClientShowNotification(title: null,
                                                      message: tilesToModify.Count + " tiles copied!",
                                                      color: NotificationColor.Good);

            SaveLastBufferEntry();
        }

        public static void ClientPaste(Vector2Ushort tilePosition)
        {
            if (lastBufferEntry is null)
            {
                return;
            }

            var bufferEntry = lastBufferEntry.Value;
            if (ClientEditorAreaSelectorHelper.Instance is not null)
            {
                NotificationSystem.ClientShowNotification(
                    title: null,
                    message:
                    "You're already in object placement mode."
                    + ObjectPlacementGuide,
                    color: NotificationColor.Neutral);
                return;
            }

            NotificationSystem.ClientShowNotification(
                title: null,
                message:
                $"{bufferEntry.Entries.Count} tiles ready for paste!"
                + ObjectPlacementGuide,
                color: NotificationColor.Good);

            var originalSize = bufferEntry.Size;

            // ReSharper disable once ObjectCreationAsStatement
            new ClientEditorAreaSelectorHelper(tilePosition,
                                               originalSize,
                                               selectedCallback: PlaceSelectedCallback);

            void PlaceSelectedCallback(Vector2Ushort selectedTilePosition)
            {
                var entries = bufferEntry.Entries
                                         .Select(e => e.ApplyOffset(selectedTilePosition))
                                         .ToList();
                TerrainEditingSystem.ClientModifyTerrain(entries);
            }
        }

        public static void DestroyInstance()
        {
            ClientEditorAreaSelectorHelper.Instance?.Destroy();
        }

        private static void SaveLastBufferEntry()
        {
            if (lastBufferEntry.HasValue)
            {
                ClientStorage.Save(lastBufferEntry.Value);
            }
        }

        private static void TryLoadBufferEntry()
        {
            if (ClientStorage.TryLoad(out BufferEntry loaded))
            {
                lastBufferEntry = loaded;
            }
        }

        private readonly struct BufferEntry
        {
            public BufferEntry(
                List<TerrainEditingSystem.TileModifyRequest> entries,
                Vector2Ushort size)
            {
                this.Entries = entries;
                this.Size = size;
            }

            public List<TerrainEditingSystem.TileModifyRequest> Entries { get; }

            public Vector2Ushort Size { get; }
        }
    }
}