namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering
{
    using System;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    /// <summary>
    /// Special blend-animation renderer - blends each two frames linearly.
    /// For example, three frames will be blended like this: 0->1, 1->2, 2->0, repeat.
    /// </summary>
    public class ClientComponentSpriteSheetBlendAnimator : ClientComponent
    {
        private static readonly EffectResource EffectResourceDrawBlendAnimation
            = new EffectResource("DrawBlendAnimation");

        private double currentTime;

        private double frameDurationSeconds;

        private int frameIndex;

        private ITextureResource[] framesTextureResources;

        private RenderingMaterial renderingMaterial;

        private IComponentSpriteRenderer spriteRenderer;

        private double totalDurationSeconds;

        public int FrameIndex => this.frameIndex;

        public int FramesCount => this.framesTextureResources.Length;

        public void Reset()
        {
            this.currentTime = 0f;
        }

        public void Setup(
            IComponentSpriteRenderer spriteRenderer,
            ITextureResource[] framesTextureResources,
            double frameDurationSeconds)
        {
            if (framesTextureResources == null
                || framesTextureResources.Length == 0)
            {
                throw new Exception("Incorrect sprite sheet");
            }

            this.spriteRenderer = spriteRenderer;
            this.renderingMaterial = RenderingMaterial.Create(EffectResourceDrawBlendAnimation);
            spriteRenderer.RenderingMaterial = this.renderingMaterial;

            this.framesTextureResources = framesTextureResources;
            this.frameDurationSeconds = frameDurationSeconds;
            this.totalDurationSeconds = frameDurationSeconds * framesTextureResources.Length;

            this.Reset();

            this.Update(0f);
        }

        public override void Update(double deltaTime)
        {
            if (this.framesTextureResources == null)
            {
                throw new Exception("Sprite sheet animator is not setup");
            }

            this.currentTime += deltaTime;

            var frameIndexFloat = this.currentTime % this.totalDurationSeconds / this.frameDurationSeconds;
            this.frameIndex = this.totalDurationSeconds > 0
                                  ? (int)frameIndexFloat
                                  : 0;
            this.spriteRenderer.TextureResource = this.framesTextureResources[this.frameIndex];

            var nextFrameIndex = this.frameIndex + 1;
            if (nextFrameIndex >= this.FramesCount)
            {
                nextFrameIndex = 0;
            }

            this.renderingMaterial.EffectParameters
                .Set("SpriteTextureArraySliceNext", nextFrameIndex)
                .Set("SpriteLerp",                  (float)(frameIndexFloat % 1.0));
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.Reset();
        }
    }
}