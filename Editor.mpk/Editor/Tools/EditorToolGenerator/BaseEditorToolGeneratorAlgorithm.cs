namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class BaseEditorToolGeneratorAlgorithm : ProtoEntity
    {
        private List<ViewModelHeightSetting> heightSettings;

        private IReadOnlyList<ProtoTileForHeight> protoTilePerHeight;

        public abstract ITextureResource Icon { get; }

        public void ClientGenerate(long seed, BoundsUshort worldBounds, byte startHeight)
        {
            DialogWindow.ShowDialog(
                "Generate",
                new TextBlock()
                {
                    Text = "Selected world part will be DELETED!"
                           + Environment.NewLine
                           + "There is NO undo for this action. Are you sure?",
                    TextAlignment = TextAlignment.Center
                },
                okAction: () => this.ClientGenerateMap(seed, worldBounds, startHeight),
                focusOnCancelButton: true,
                hideCancelButton: false);
        }

        public void Setup(List<ViewModelHeightSetting> heightSettings)
        {
            this.heightSettings = heightSettings;
        }

        protected void ClientApplyMap(Vector2Ushort worldOffset, double[,] map, byte startHeight)
        {
            var width = map.GetLength(0);
            var height = map.GetLength(1);

            var modifyRequests = new List<TileModifyRequest>();

            for (ushort y = 0; y < height; y++)
            for (ushort x = 0; x < width; x++)
            {
                var value = map[x, y];
                var protoTile = this.GetProtoTile(value);
                var tileHeight = (byte)(startHeight + this.GetTileHeight(value));
                if (tileHeight > ScriptingConstants.MaxTileHeight)
                {
                    tileHeight = ScriptingConstants.MaxTileHeight;
                }

                var tilePosition = new Vector2Ushort((ushort)(worldOffset.X + x), (ushort)(worldOffset.Y + y));
                modifyRequests.Add(new TileModifyRequest(tilePosition, protoTile.SessionIndex, tileHeight));
            }

            foreach (var chunk in modifyRequests.Batch(size: 200))
            {
                this.CallServer(_ => _.ServerRemote_ApplyMap(chunk.ToList()));
            }

            this.CallServer(_ => _.ServerRemote_FixMap());
        }

        protected abstract void ClientGenerateMap(long seed, BoundsUshort worldBounds, byte startHeight);

        protected override void PrepareProto()
        {
            base.PrepareProto();

            var protoTileForHeights = new List<ProtoTileForHeight>();
            this.SetupProtoTileForHeights(protoTileForHeights);

            this.protoTilePerHeight = protoTileForHeights.AsReadOnly();
        }

        protected virtual void SetupProtoTileForHeights(List<ProtoTileForHeight> protoTileForHeights)
        {
            protoTileForHeights.Add(new ProtoTileForHeight(0,    GetProtoEntity<TileWaterSea>()));
            protoTileForHeights.Add(new ProtoTileForHeight(0.33, GetProtoEntity<TileBeachTemperate>()));
            protoTileForHeights.Add(new ProtoTileForHeight(0.66, GetProtoEntity<TileForestTemperate>()));
            protoTileForHeights.Add(new ProtoTileForHeight(0.8,  GetProtoEntity<TileRocky>()));
            protoTileForHeights.Add(new ProtoTileForHeight(1,    GetProtoEntity<TileBarren>()));
        }

        private IProtoTile GetProtoTile(double value)
        {
            foreach (var pair in this.protoTilePerHeight)
            {
                if (value <= pair.Limit)
                {
                    return pair.ProtoTile;
                }
            }

            return this.protoTilePerHeight[this.protoTilePerHeight.Count - 1].ProtoTile;
        }

        private byte GetTileHeight(double value)
        {
            for (byte index = 0; index < this.heightSettings.Count; index++)
            {
                var setting = this.heightSettings[index];
                if (value <= setting.MaxValue)
                {
                    return index;
                }
            }

            return (byte)this.heightSettings.Count;
        }

        [RemoteCallSettings(DeliveryMode.Default,
                            timeInterval: 0,
                            clientMaxSendQueueSize: byte.MaxValue)]
        private void ServerRemote_ApplyMap(IReadOnlyCollection<TileModifyRequest> modifyRequests)
        {
            var worldService = Server.World;

            foreach (var request in modifyRequests)
            {
                var protoTile = worldService.GetProtoTileBySessionIndex(request.ProtoTileSessionIndex);

                worldService.SetTileData(
                    request.TilePosition,
                    protoTile,
                    tileHeight: request.TileHeight,
                    isSlope: false,
                    isCliff: false);
            }
        }

        private void ServerRemote_FixMap()
        {
            Server.World.FixMapTilesRecentlyModified();
        }

        protected readonly struct ProtoTileForHeight : IRemoteCallParameter
        {
            public readonly double Limit;

            public readonly IProtoTile ProtoTile;

            public ProtoTileForHeight(double limit, IProtoTile protoTile)
            {
                this.ProtoTile = protoTile;
                this.Limit = limit;
            }
        }

        private readonly struct TileModifyRequest : IRemoteCallParameter
        {
            public readonly byte ProtoTileSessionIndex;

            public readonly byte TileHeight;

            public readonly Vector2Ushort TilePosition;

            public TileModifyRequest(Vector2Ushort tilePosition, byte protoTileSessionIndex, byte tileHeight)
            {
                this.TilePosition = tilePosition;
                this.ProtoTileSessionIndex = protoTileSessionIndex;
                this.TileHeight = tileHeight;
            }
        }
    }
}