namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering
{
    using System;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ClientComponentSpriteSheetAnimator : ClientComponent
    {
        private double currentTime;

        private double frameDurationSeconds;

        private int frameIndex;

        private ITextureResource[] framesTextureResources;

        private IComponentSpriteRenderer spriteRenderer;

        private double timeOffset;

        private double totalDurationSeconds;

        public int FrameIndex => this.frameIndex;

        public int FramesCount => this.framesTextureResources.Length;

        public bool IsManualUpdate { get; set; }

        //public ITextureResource[] FramesTextureResources => this.framesTextureResources;

        /// <summary>
        /// Generate textures from atlas.
        /// </summary>
        /// <param name="textureAtlasResource">Texture atlas.</param>
        /// <param name="columns">(optional)</param>
        /// <param name="rowsCount">(optional)</param>
        /// <param name="autoInverse">
        /// (optional) Enable this if you want to append reverse animation. Please note - first and last
        /// frame will be displayed twice.
        /// </param>
        /// <returns></returns>
        public static ITextureResource[] CreateAnimationFrames(
            ITextureAtlasResource textureAtlasResource,
            byte? columns = null,
            byte? rowsCount = null,
            byte? onlySpecificRow = null,
            bool autoInverse = false)
        {
            var atlasColumnsCount = columns ?? textureAtlasResource.AtlasSize.ColumnsCount;
            var atlasRowsCount = rowsCount ?? textureAtlasResource.AtlasSize.RowsCount;

            int chunksCount;
            if (onlySpecificRow.HasValue)
            {
                chunksCount = atlasColumnsCount;
            }
            else
            {
                chunksCount = atlasColumnsCount * atlasRowsCount;
            }

            var result = new ITextureResource[chunksCount * (autoInverse ? 2 : 1)];

            if (onlySpecificRow.HasValue)
            {
                var row = onlySpecificRow.Value;
                for (byte column = 0; column < atlasColumnsCount; column++)
                {
                    result[column] = textureAtlasResource.Chunk(column, row);
                }
            }
            else
            {
                for (byte row = 0; row < atlasRowsCount; row++)
                {
                    for (byte column = 0; column < atlasColumnsCount; column++)
                    {
                        result[row * atlasColumnsCount + column] = textureAtlasResource.Chunk(column, row);
                    }
                }
            }

            if (autoInverse)
            {
                for (var i = 0; i < chunksCount; i++)
                {
                    result[chunksCount + i] = result[chunksCount - i - 1];
                }
            }

            return result;
        }

        public void ForceUpdate(double deltaTime)
        {
            if (this.framesTextureResources == null)
            {
                throw new Exception("Sprite sheet animator is not setup");
            }

            this.currentTime += deltaTime;
            this.frameIndex = this.totalDurationSeconds > 0
                                  ? (int)(this.currentTime % this.totalDurationSeconds / this.frameDurationSeconds)
                                  : 0;
            var textureResource = this.framesTextureResources[this.frameIndex];
            this.spriteRenderer.TextureResource = textureResource;
            //Api.Logger.Dev("Sprite animation frame: #"
            //               + this.frameIndex
            //               + " texture: "
            //               + textureResource
            //               + " total duration: "
            //               + this.currentTime.ToString("F3"));
        }

        public void Reset()
        {
            this.currentTime = this.timeOffset;
        }

        public void Setup(
            IComponentSpriteRenderer spriteRenderer,
            ITextureResource[] framesTextureResources,
            double frameDurationSeconds,
            bool isManualUpdate = false,
            int? initialFrameOffset = 0,
            bool randomizeInitialFrame = false)
        {
            if (framesTextureResources == null
                || framesTextureResources.Length == 0)
            {
                throw new Exception("Incorrect sprite sheet");
            }

            this.spriteRenderer = spriteRenderer;
            this.framesTextureResources = framesTextureResources;
            this.frameDurationSeconds = frameDurationSeconds;
            this.totalDurationSeconds = frameDurationSeconds * framesTextureResources.Length;
            this.IsManualUpdate = isManualUpdate;

            if (!initialFrameOffset.HasValue
                && randomizeInitialFrame)
            {
                initialFrameOffset = RandomHelper.Next(0, this.FramesCount);
            }

            this.timeOffset = (initialFrameOffset ?? 0) * frameDurationSeconds;

            this.Reset();

            this.Update(0f);
        }

        public override void Update(double deltaTime)
        {
            if (!this.IsManualUpdate)
            {
                this.ForceUpdate(deltaTime);
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.Reset();
        }
    }
}