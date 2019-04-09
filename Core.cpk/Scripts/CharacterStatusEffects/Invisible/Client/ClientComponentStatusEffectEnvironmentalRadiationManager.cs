namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Invisible.Client
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.StatusEffects;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ClientComponentStatusEffectEnvironmentalRadiationManager : ClientComponent
    {
        // Client will lerp intensity from 0 to 1 during 2 seconds.
        private const double IntensityLerpDuration = 2;

        private static ClientComponentStatusEffectEnvironmentalRadiationManager instance;

        private static double targetIntensity;

        private double currentIntensity;

        private PostEffectRadiation postEffect;

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
                    instance = Client.Scene.CreateSceneObject("Radiation visualizer")
                                     .AddComponent<ClientComponentStatusEffectEnvironmentalRadiationManager>();
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
            this.soundEffect.Intensity = this.currentIntensity;

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

            this.soundEffect.Destroy();
            this.soundEffect = null;

            if (instance == this)
            {
                instance = null;
            }
        }

        protected override void OnEnable()
        {
            this.postEffect = ClientPostEffectsManager.Add<PostEffectRadiation>();
            this.postEffect.Intensity = this.currentIntensity = 0;

            this.soundEffect = this.SceneObject.AddComponent<ClientComponentSoundEffect>();
        }

        /// <summary>
        /// Plays Geiger sound effect randomly with the desired intensity.
        /// </summary>
        private class ClientComponentSoundEffect : ClientComponent
        {
            private const float Volume = 0.15f;

            private static readonly IAudioClientService Audio = Api.Client.Audio;

            private static readonly SoundResource SoundResourceClickSeries
                = new SoundResource("StatusEffects/Invisible/EnvironmentalRadiation/GeigerSeries1");

            private static readonly SoundResource SoundResourceOneClick
                = new SoundResource("StatusEffects/Invisible/EnvironmentalRadiation/GeigerClick1");

            public double Intensity { get; set; }

            public override void Update(double deltaTime)
            {
                // calculate probability of playing sound in this frame
                // we're using non-linear function
                var realProbability = Math.Pow(this.Intensity, 0.5) * 0.4;

                // this sound effect is designed for 60 FPS,
                // for lower framerate it should play with higher probability (and vice versa)
                var timeFactor = deltaTime * 60.0;
                var probability = realProbability * timeFactor;

                // clamp probability (on full probability it will sound every frame, not randomly)
                probability = Math.Min(probability, 0.5);

                // calculate chance to replace single click with a series of clicks
                var series = Math.Max(0, this.Intensity - 0.5) * 0.5;

                // volume correction
                var finalVolume = Volume * (0.5f + (float)this.Intensity / 2f);

                if (RandomHelper.RollWithProbability(probability))
                {
                    if (RandomHelper.RollWithProbability(series))
                    {
                        // chance to play a series
                        Audio.PlayOneShot(SoundResourceClickSeries, volume: finalVolume);
                    }
                    else
                    {
                        // single click
                        Audio.PlayOneShot(SoundResourceOneClick, volume: finalVolume);
                    }
                }

                //// uncomment to enable logging
                //if (RandomHelper.RollWithProbability(0.1))
                //{
                //    Api.Logger.Write(
                //            "Intensity: "
                //            + Math.Round(this.Intensity, 2)
                //            + ", Probability: "
                //            + Math.Round(probability, 2)
                //            + ", Basic probability: "
                //            + Math.Round(realProbability, 2)
                //            + ", Series chance: "
                //            + Math.Round(series, 2)
                //            + ", Volume: "
                //            + Math.Round(finalVolume, 2)
                //        );
                //}
            }
        }
    }
}