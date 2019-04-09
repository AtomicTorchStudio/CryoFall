namespace AtomicTorch.CBND.CoreMod.ClientComponents.FX
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    public class ClientComponentOneShotLightAnimation : ClientComponent
    {
        private double duration;

        private ClientComponentSpriteLightSource lightSource;

        private double time;

        public static void Setup(ClientComponentSpriteLightSource lightSource, double duration)
        {
            lightSource.SceneObject.AddComponent<ClientComponentOneShotLightAnimation>()
                       .SetupInternal(lightSource, duration);
        }

        public override void Update(double deltaTime)
        {
            this.time += deltaTime;
            if (this.time > this.duration)
            {
                this.lightSource.Destroy();
                this.Destroy();
                return;
            }

            // calculate animation progress (from 0 to 1)
            var alpha = (this.duration - this.time) / this.duration;
            this.lightSource.Opacity = alpha;
        }

        private void SetupInternal(ClientComponentSpriteLightSource lightSource, double duration)
        {
            this.lightSource = lightSource;
            this.duration = duration;
            this.time = 0;
            this.Update(0);
        }
    }
}