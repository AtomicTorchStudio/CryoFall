namespace AtomicTorch.CBND.CoreMod.Systems.TeleportsSystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.Bloom;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Helpers;

    /// <summary>
    /// This effects adds post-processing only (bloom and fade-in/out) and used together with the primary effect.
    /// </summary>
    public class ClientComponentTeleportationEffectCurrentCharacter : ClientComponent
    {
        private static readonly BloomSettings BloomSettings
            = new(name: "Teleportation current character",
                  bloomThreshold: 0.85f,
                  blurAmount: 3,
                  bloomIntensity: 6,
                  baseIntensity: 1,
                  bloomSaturation: 1,
                  baseSaturation: 1);

        private double durationAnimation;

        private double durationTeleportation;

        private bool isTeleportationOut;

        private BloomPostEffect postEffectBloom;

        private TeleportationCurrentCharacterPostEffect postEffectFade;

        private double time;

        public void Setup(double duration, double teleportationDelay)
        {
            this.durationAnimation = duration;
            this.durationTeleportation = teleportationDelay;
            this.isTeleportationOut = true;
            this.Update(0);
        }

        public override void Update(double deltaTime)
        {
            this.time += deltaTime;
            this.time = Math.Min(this.time, this.durationTeleportation);

            this.postEffectFade.IsSuppressed = false;
            this.postEffectBloom.IsSuppressed = false;

            var bloomProgress = this.time / this.durationAnimation;
            bloomProgress = MathHelper.Clamp(bloomProgress, 0, 1);
            if (!this.isTeleportationOut)
            {
                bloomProgress = 1 - bloomProgress;
            }

            var fadeDelay = this.isTeleportationOut
                                ? this.durationAnimation / 3.0
                                : this.durationAnimation;
            var fadeDuration = this.isTeleportationOut
                                   ? this.durationTeleportation - this.durationAnimation
                                   : (this.durationTeleportation - this.durationAnimation) / 2.0;

            var fadeProgress = (this.time - fadeDelay)
                               / fadeDuration;
            fadeProgress = MathHelper.Clamp(fadeProgress, 0, 1);
            if (!this.isTeleportationOut)
            {
                fadeProgress = 1 - fadeProgress;
            }

            this.postEffectBloom.Intensity = bloomProgress;
            this.postEffectFade.Intensity = fadeProgress;

            if (this.time < this.durationTeleportation)
            {
                return;
            }

            if (this.isTeleportationOut)
            {
                this.isTeleportationOut = false;
                this.time = this.durationTeleportation - this.durationAnimation;
            }
            else
            {
                // teleportation completed!
                this.Destroy();
            }
        }

        protected override void OnDisable()
        {
            this.postEffectBloom.Destroy();
            this.postEffectFade.Destroy();
        }

        protected override void OnEnable()
        {
            this.postEffectFade = ClientPostEffectsManager.Add<TeleportationCurrentCharacterPostEffect>();
            this.postEffectFade.Order = PostEffectsOrder.StatusEffects + 1;

            this.postEffectBloom = ClientPostEffectsManager.Add<BloomPostEffect>();
            this.postEffectBloom.Setup(BloomSettings);
            this.postEffectBloom.Order = this.postEffectFade.Order - 1;

            this.postEffectFade.IsSuppressed = true;
            this.postEffectBloom.IsSuppressed = true;
        }
    }
}