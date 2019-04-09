namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs.Client
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.StatusEffects;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ClientComponentStatusEffectPsiManager : ClientComponent
    {
        // Client will lerp intensity from 0 to 1 during 3 seconds.
        private const double IntensityLerpDuration = 3;

        private static ClientComponentStatusEffectPsiManager instance;

        private static double targetIntensity;

        private double currentIntensity;

        private PostEffectPsi postEffect;

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
                    instance = Client.Scene.CreateSceneObject("Psi visualizer")
                                     .AddComponent<ClientComponentStatusEffectPsiManager>();
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

            this.soundEffect.Intensity = this.currentIntensity;
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
            this.soundEffect.Destroy();
            this.soundEffect = null;

            this.postEffect.Destroy();
            this.postEffect = null;

            if (instance == this)
            {
                instance = null;
            }
        }

        protected override void OnEnable()
        {
            this.soundEffect = this.SceneObject.AddComponent<ClientComponentSoundEffect>();
            this.postEffect = ClientPostEffectsManager.Add<PostEffectPsi>();
            this.postEffect.Intensity = this.currentIntensity = 0;
        }

        /// <summary>
        /// Plays looped sound effect with the desired intensity.
        /// </summary>
        private class ClientComponentSoundEffect : ClientComponent
        {
            private const float Volume = 0.5f;

            private static readonly SoundResource SoundResourcePsiProcess
                = new SoundResource("StatusEffects/Debuffs/Psi/Process");

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
                    var level = Math.Sqrt(Math.Max(0, this.intensity - 0.12));
                    this.soundEmitter.Volume = (float)level * Volume;
                }
            }

            protected override void OnEnable()
            {
                base.OnEnable();
                this.soundEmitter = Client.Audio.CreateSoundEmitter(
                    this.SceneObject,
                    SoundResourcePsiProcess,
                    isLooped: true,
                    is3D: false);
                this.soundEmitter.Volume = 0;
                this.soundEmitter.Play();
            }
        }
    }
}