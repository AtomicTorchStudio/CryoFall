namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs.Client
{
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ClientComponentStatusEffectDazedManager : ClientComponent
    {
        private const double IntensityLerpDurationIn = 0.1;

        private const double IntensityLerpDurationOut = 1.5;

        private static ClientComponentStatusEffectDazedManager instance;

        private static double targetIntensity;

        private double currentIntensity;

        private ClientComponentSoundEffect soundEffect;

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
                    && instance == null)
                {
                    // ensure instance exist       
                    instance = Client.Scene.CreateSceneObject("Dazed visualizer")
                                     .AddComponent<ClientComponentStatusEffectDazedManager>();
                }
            }
        }

        public override void Update(double deltaTime)
        {
            var targetIntensity = TargetIntensity;

            // clamp target intensity
            targetIntensity = targetIntensity > 0.1
                                  ? 1
                                  : targetIntensity;

            // lerp intensity
            this.currentIntensity = MathHelper.LerpTowards(
                from: this.currentIntensity,
                to: targetIntensity,
                maxStep: deltaTime
                         / (this.currentIntensity < targetIntensity
                                ? IntensityLerpDurationIn
                                : IntensityLerpDurationOut));

            this.soundEffect.Intensity = this.currentIntensity;

            if (targetIntensity == 0d
                && this.currentIntensity == 0d)
            {
                // auto delete when effect is not needed more
                this.Destroy();
            }
        }

        protected override void OnDisable()
        {
            this.soundEffect.Destroy();
            this.soundEffect = null;

            if (instance == this)
            {
                instance = null;
            }
        }

        protected override void OnEnable()
        {
            this.soundEffect = this.SceneObject.AddComponent<ClientComponentSoundEffect>();
        }

        /// <summary>
        /// Plays Dazed sound effect in loop.
        /// </summary>
        private class ClientComponentSoundEffect : ClientComponent
        {
            private const float Volume = 0.75f;

            private static readonly SoundResource SoundResourceDazedProcess
                = new SoundResource("StatusEffects/Debuffs/Dazed/Process");

            private double intensity;

            private IComponentSoundEmitter soundEmitter;

            public double Intensity
            {
                get => this.intensity;
                set
                {
                    if (this.intensity == value)
                    {
                        return;
                    }

                    this.intensity = value;
                    this.soundEmitter.Volume = (float)this.intensity * Volume;
                }
            }

            protected override void OnEnable()
            {
                base.OnEnable();
                this.soundEmitter = Client.Audio.CreateSoundEmitter(
                    this.SceneObject,
                    SoundResourceDazedProcess,
                    isLooped: true,
                    is3D: false);
                this.soundEmitter.Volume = 0;
                this.soundEmitter.Play();
            }
        }
    }
}