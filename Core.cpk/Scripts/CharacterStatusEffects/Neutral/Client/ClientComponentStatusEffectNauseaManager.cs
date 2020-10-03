namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral.Client
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects;
    using AtomicTorch.CBND.CoreMod.ClientComponents.PostEffects.StatusEffects;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ClientComponentStatusEffectNauseaManager : ClientComponent
    {
        // Client will lerp intensity from 0 to 1 during 3 seconds.
        private const double IntensityLerpDuration = 3;

        private static ClientComponentStatusEffectNauseaManager instance;

        private static double targetIntensity;

        private double currentIntensity;

        private PostEffectNausea postEffect;

        public static void Refresh()
        {
            var character = ClientCurrentCharacterHelper.Character;
            var intensity = Math.Max(
                character.SharedGetStatusEffectIntensity<StatusEffectDrunk>(),
                character.SharedGetStatusEffectIntensity<StatusEffectNausea>());

            intensity = MathHelper.Clamp(intensity, min: 0, max: 1);
            if (targetIntensity == intensity)
            {
                return;
            }

            targetIntensity = intensity;

            if (targetIntensity > 0
                && instance is null)
            {
                // ensure instance exist       
                instance = Client.Scene.CreateSceneObject("Drunk visualizer")
                                 .AddComponent<ClientComponentStatusEffectNauseaManager>();
            }
        }

        public override void Update(double deltaTime)
        {
            Refresh();

            // lerp intensity
            this.currentIntensity = MathHelper.LerpTowards(
                from: this.currentIntensity,
                to: targetIntensity,
                maxStep: deltaTime / IntensityLerpDuration);

            this.postEffect.Intensity = this.currentIntensity;

            if (targetIntensity == 0d
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
            this.postEffect = ClientPostEffectsManager.Add<PostEffectNausea>();
            this.postEffect.Intensity = this.currentIntensity = 0;
        }
    }
}