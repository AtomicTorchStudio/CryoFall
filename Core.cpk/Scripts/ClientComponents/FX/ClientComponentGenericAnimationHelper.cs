namespace AtomicTorch.CBND.CoreMod.ClientComponents.FX
{
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    public class ClientComponentGenericAnimationHelper : ClientComponent
    {
        private double duration;

        private double time;

        private DelegateUpdateCallback updateCallback;

        public delegate void DelegateUpdateCallback(float alpha);

        public static void Setup(
            IClientSceneObject sceneObject,
            double duration,
            DelegateUpdateCallback updateCallback)
        {
            sceneObject.AddComponent<ClientComponentGenericAnimationHelper>()
                       .SetupInternal(duration, updateCallback);
        }

        public override void Update(double deltaTime)
        {
            this.time += deltaTime;
            if (this.time > this.duration)
            {
                this.Destroy();
                return;
            }

            var alpha = (float)(this.time / this.duration);
            this.updateCallback(alpha);
        }

        private void SetupInternal(double duration, DelegateUpdateCallback updateCallback)
        {
            this.duration = duration;
            this.updateCallback = updateCallback;

            this.time = 0;
            this.Update(0);
        }
    }
}