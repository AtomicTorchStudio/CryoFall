namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs.Client
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.StatusEffects;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ClientComponentStatusEffectColdManager : ClientComponent
    {
        // Client will lerp intensity from 0 to 1 during 4 seconds.
        private const double IntensityLerpDuration = 4;

        private static ClientComponentStatusEffectColdManager instance;

        private static double targetIntensity;

        private double currentIntensity;

        private PostEffectCold postEffect;

        public static double TargetIntensity
        {
            get => targetIntensity;
            set
            {
                value = MathHelper.Clamp(value, min: 0, max: 1);
                if (targetIntensity == value)
                {
                    return;
                }

                targetIntensity = value;

                if (targetIntensity > 0
                    && instance is null)
                {
                    // ensure instance exist       
                    instance = Client.Scene.CreateSceneObject("Cold visualizer")
                                     .AddComponent<ClientComponentStatusEffectColdManager>();
                }
            }
        }

        public override void Update(double deltaTime)
        {
            // lerp intensity
            this.currentIntensity = MathHelper.LerpTowards(
                from: this.currentIntensity,
                to: TargetIntensity,
                maxStep: deltaTime / IntensityLerpDuration);

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
            this.postEffect = ClientPostEffectsManager.Add<PostEffectCold>();
            this.postEffect.Intensity = this.currentIntensity = 0;
        }
    }
}