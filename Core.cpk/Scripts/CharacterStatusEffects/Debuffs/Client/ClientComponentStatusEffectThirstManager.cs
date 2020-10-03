namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs.Client
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ClientComponentStatusEffectThirstManager : ClientComponent
    {
        private static ClientComponentStatusEffectThirstManager instance;

        private static double intensity;

        private ClientComponentEffectPlayer componentPlayer;

        public static double Intensity
        {
            get => intensity;
            set
            {
                value = MathHelper.Clamp(value, min: 0, max: 1);
                if (intensity == value)
                {
                    return;
                }

                intensity = value;

                if (intensity > 0
                    && instance is null)
                {
                    // ensure instance exist       
                    instance = Client.Scene.CreateSceneObject("Thirst visualizer")
                                     .AddComponent<ClientComponentStatusEffectThirstManager>();
                }
            }
        }

        public override void Update(double deltaTime)
        {
            if (Intensity <= 0)
            {
                // auto delete when effect is not needed more
                this.Destroy();
            }
        }

        protected override void OnDisable()
        {
            this.componentPlayer.Destroy();
            this.componentPlayer = null;

            if (instance == this)
            {
                instance = null;
            }
        }

        protected override void OnEnable()
        {
            this.componentPlayer = this.SceneObject.AddComponent<ClientComponentEffectPlayer>();
        }

        private class ClientComponentEffectPlayer : ClientComponent
        {
            private const double PlayInterval = 3;

            private const float Volume = 0.5f;

            private static readonly ReadOnlySoundResourceSet SoundsOccasional
                = new SoundResourceSet().Add("StatusEffects/Debuffs/Thirst/Occasional")
                                        .ToReadOnly();

            private double lastPlayedTime = double.MinValue;

            public override void Update(double deltaTime)
            {
                var now = Client.Core.ClientRealTime;

                if (now >= this.lastPlayedTime + PlayInterval)
                {
                    this.lastPlayedTime = now;
                    this.PlaySound();
                    this.FlickerStatusIcon();
                }
            }

            private void FlickerStatusIcon()
            {
                HUDStatusEffectsBar.Instance?.Flicker<StatusEffectThirst>();
            }

            private void PlaySound()
            {
                var randomSound = SoundsOccasional.GetSound(
                    repetitionProtectionKey: typeof(ClientComponentEffectPlayer));
                Api.Client.Audio.PlayOneShot(randomSound, Volume);
            }
        }
    }
}