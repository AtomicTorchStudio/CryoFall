namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering
{
    using System;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ClientComponentDoorSpriteSheetAnimator : ClientComponent
    {
        private int currentFrame;

        private double currentTime;

        private double frameDurationSeconds;

        private ITextureResource[] framesTextureResources;

        private bool isPositiveDirection = true;

        private IComponentSpriteRenderer spriteRenderer;

        private double totalDurationSeconds;

        public ClientComponentDoorSpriteSheetAnimator()
        {
        }

        public ITextureResource[] FramesTextureResources => this.framesTextureResources;

        public bool IsPositiveDirection => this.isPositiveDirection;

        public void SetCurrentFrame(int frameNumber)
        {
            this.SetCurrentFrameInternal(frameNumber);
            this.currentTime = this.currentFrame * this.frameDurationSeconds;
        }

        public void Setup(
            IComponentSpriteRenderer spriteRenderer,
            ITextureResource[] framesTextureResources,
            double frameDurationSeconds)
        {
            if (framesTextureResources is null
                || framesTextureResources.Length == 0)
            {
                throw new Exception("Incorrect sprite sheet");
            }

            this.spriteRenderer = spriteRenderer;
            this.framesTextureResources = framesTextureResources;
            this.frameDurationSeconds = frameDurationSeconds;
            this.totalDurationSeconds = frameDurationSeconds * framesTextureResources.Length;
            this.currentTime = 0f;

            this.spriteRenderer.TextureResource = this.framesTextureResources[0];
            this.Stop();
        }

        public void Start(bool isPositiveDirection)
        {
            this.isPositiveDirection = isPositiveDirection;
            this.IsEnabled = true;
        }

        public void Stop()
        {
            this.IsEnabled = false;
        }

        public override void Update(double deltaTime)
        {
            if (this.framesTextureResources is null)
            {
                throw new Exception("Sprite sheet animator is not setup");
            }

            if (this.isPositiveDirection)
            {
                this.currentTime += deltaTime;
                if (this.currentTime >= this.totalDurationSeconds)
                {
                    // completed
                    this.IsEnabled = false;
                    this.currentTime = this.totalDurationSeconds;
                }
            }
            else
            {
                // negative direction
                this.currentTime -= deltaTime;
                if (this.currentTime <= 0)
                {
                    // completed
                    this.IsEnabled = false;
                    this.currentTime = 0;
                }
            }

            var frameNumber = (int)(this.currentTime / this.frameDurationSeconds);
            this.SetCurrentFrameInternal(frameNumber);
        }

        private void SetCurrentFrameInternal(int frameNumber)
        {
            if (this.currentFrame == frameNumber)
            {
                return;
            }

            if (frameNumber >= this.framesTextureResources.Length)
            {
                frameNumber = this.framesTextureResources.Length - 1;
            }
            else if (frameNumber < 0)
            {
                frameNumber = 0;
            }

            if (this.currentFrame == frameNumber)
            {
                return;
            }

            this.currentFrame = frameNumber;
            this.spriteRenderer.TextureResource = this.framesTextureResources[frameNumber];
        }
    }
}