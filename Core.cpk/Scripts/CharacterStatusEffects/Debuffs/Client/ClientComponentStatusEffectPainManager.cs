namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs.Client
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.StatusEffects;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ClientComponentStatusEffectPainManager : ClientComponent
    {
        // Duration to lerp intensity from 1 to 0:
        private const double IntensityLerpDurationDown = 2;

        // Duration to lerp intensity from 0 to 1:
        private const double IntensityLerpDurationUp = 0.5;

        private static ClientComponentStatusEffectPainManager instance;

        private static double targetIntensity;

        private double currentIntensity;

        private PostEffectPain postEffect;

        public static double TargetIntensity
        {
            get => targetIntensity;
            set
            {
                value = value > 0
                            ? 1
                            : 0;

                //value = MathHelper.Clamp(value, min: 0, max: 1);
                if (targetIntensity == value)
                {
                    return;
                }

                targetIntensity = value;

                if (targetIntensity > 0
                    && instance is null)
                {
                    // ensure instance exist       
                    instance = Client.Scene.CreateSceneObject("Pain visualizer")
                                     .AddComponent<ClientComponentStatusEffectPainManager>();
                }
            }
        }

        public override void Update(double deltaTime)
        {
            // lerp intensity
            this.currentIntensity = MathHelper.LerpTowards(
                from: this.currentIntensity,
                to: TargetIntensity,
                maxStep: TargetIntensity > this.currentIntensity
                             ? deltaTime / IntensityLerpDurationUp
                             : deltaTime / IntensityLerpDurationDown);

            this.postEffect.Intensity = this.currentIntensity;

            if (TargetIntensity == 0d
                && this.currentIntensity == 0d)
            {
                // auto delete when effect is not needed more
                this.Destroy();
            }
        }

        protected override void OnDisable()
        {
            this.postEffect.Destroy();
            this.postEffect = null;

            if (instance == this)
            {
                instance = null;
            }
        }

        protected override void OnEnable()
        {
            this.postEffect = ClientPostEffectsManager.Add<PostEffectPain>();
            this.postEffect.Intensity = this.currentIntensity = 0;
        }
    }
}