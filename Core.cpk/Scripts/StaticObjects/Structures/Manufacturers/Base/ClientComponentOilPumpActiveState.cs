namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    /// <summary>
    /// This component manages active state of the oil pump - animation and sound effects.
    /// </summary>
    public class ClientComponentOilPumpActiveState : ClientComponent
    {
        private double activatedTime;

        private bool isActive;

        private int lastFrameIndex;

        private IComponentSpriteRenderer overlayRenderer;

        private bool playSounds;

        private ClientComponentSpriteSheetAnimator spriteSheetAnimator;

        private IStaticWorldObject worldObject;

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;

                if (this.isActive)
                {
                    this.activatedTime = Client.Core.ClientRealTime;
                }

                this.spriteSheetAnimator.Reset();
                this.lastFrameIndex = -1;
                this.Update(deltaTime: 0);
            }
        }

        public void Setup(
            IComponentSpriteRenderer overlayRenderer,
            ClientComponentSpriteSheetAnimator spriteSheetAnimator,
            IStaticWorldObject worldObject,
            bool playSounds)
        {
            this.overlayRenderer = overlayRenderer;
            this.spriteSheetAnimator = spriteSheetAnimator;
            this.worldObject = worldObject;
            this.playSounds = playSounds;
        }

        public override void Update(double deltaTime)
        {
            if (this.isActive)
            {
                if (!this.overlayRenderer.IsEnabled)
                {
                    // enable components
                    this.overlayRenderer.IsEnabled
                        = this.spriteSheetAnimator.IsEnabled
                              = true;

                    this.spriteSheetAnimator.Update(0);
                }
            }
            else if (!this.overlayRenderer.IsEnabled)
            {
                // not active, rendering is not enabled 
                return;
            }

            var frameIndex = this.spriteSheetAnimator.FrameIndex;
            var isSameFrame = frameIndex == this.lastFrameIndex;
            this.lastFrameIndex = frameIndex;

            if (!this.isActive
                && frameIndex == 0)
            {
                // stop!
                this.overlayRenderer.IsEnabled = this.spriteSheetAnimator.IsEnabled = false;
                return;
            }

            //Logger.WriteDev("Frame index: " + frameIndex);

            if (this.playSounds
                && !isSameFrame
                && frameIndex == 0)
            {
                Client.Audio.PlayOneShot(
                    this.worldObject.ProtoStaticWorldObject
                        .SharedGetObjectSoundPreset()
                        .GetSound(ObjectSound.Active),
                    this.worldObject);
            }
        }
    }
}