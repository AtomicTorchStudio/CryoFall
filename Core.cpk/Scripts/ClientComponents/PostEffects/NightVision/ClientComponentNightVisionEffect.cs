namespace AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.NightVision
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.Bloom;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    /// <summary>
    /// This component manages the post effects required for the night vision.
    /// 1. It increases ambient light for the lighting system.
    /// 2. It adds Bloom post-effect (with custom parameters) to add some softness/blurness.
    /// 3. It adds Night Vision post-effect.
    /// </summary>
    public class ClientComponentNightVisionEffect : ClientComponent
    {
        private const double AdditionalAmbientLight = 0.8;

        private const double AdditionalAmbientLightAdditiveFraction = 0.35;

        private static readonly BloomSettings BloomSettings
            = new BloomSettings(name: "Night Vision",
                                bloomThreshold: 0,
                                blurAmount: 3,
                                bloomIntensity: 1,
                                baseIntensity: 1,
                                bloomSaturation: 1,
                                baseSaturation: 1);

        private bool isLightAdjusted;

        private BloomPostEffect postEffectBloom;

        private NightVisionPostEffect postEffectNightVision;

        public override void Update(double deltaTime)
        {
            if (this.isLightAdjusted
                || !this.postEffectBloom.IsCanRender
                || !this.postEffectNightVision.IsCanRender)
            {
                return;
            }

            this.isLightAdjusted = true;
            ClientComponentLightingRenderer.AdditionalAmbientLight += AdditionalAmbientLight;
            ClientComponentLightingRenderer.AdditionalAmbightLightAdditiveFraction +=
                AdditionalAmbientLightAdditiveFraction;

            this.postEffectNightVision.IsSuppressed = false;
            this.postEffectBloom.IsSuppressed = false;
        }

        protected override void OnDisable()
        {
            if (this.isLightAdjusted)
            {
                this.isLightAdjusted = false;
                ClientComponentLightingRenderer.AdditionalAmbientLight -= AdditionalAmbientLight;
                ClientComponentLightingRenderer.AdditionalAmbightLightAdditiveFraction -=
                    AdditionalAmbientLightAdditiveFraction;
            }

            this.postEffectBloom.Destroy();
            this.postEffectNightVision.Destroy();
        }

        protected override void OnEnable()
        {
            this.postEffectNightVision = ClientPostEffectsManager.Add<NightVisionPostEffect>();
            this.postEffectNightVision.Order = PostEffectsOrder.Devices;

            this.postEffectBloom = ClientPostEffectsManager.Add<BloomPostEffect>();
            this.postEffectBloom.Setup(BloomSettings);
            this.postEffectBloom.Order = this.postEffectNightVision.Order - 1;

            this.postEffectNightVision.IsSuppressed = true;
            this.postEffectBloom.IsSuppressed = true;
        }
    }
}