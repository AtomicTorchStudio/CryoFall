namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data
{
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ProtoTileMapTexturesPreloadHelper
    {
        public static Task Prepare()
        {
            return Task.Run(Function);
        }

        /// <summary>
        /// Wait until all the tile map textures are preloaded.
        /// </summary>
        private static void Function()
        {
            var rendering = Api.Client.Rendering;
            var protoTiles = Api.FindProtoEntities<IProtoTile>();
            var tasks = new Task[protoTiles.Count + 1];

            for (var index = 0; index < protoTiles.Count; index++)
            {
                var protoTile = protoTiles[index];
                tasks[index] = rendering.PreloadTextureAsync(protoTile.WorldMapTexture);
            }

            tasks[tasks.Length - 1] = rendering.PreloadTextureAsync(ProtoTile.WorldMapTextureCliff);
            Task.WaitAll(tasks, Api.CancellationToken);
        }
    }
}