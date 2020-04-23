namespace AtomicTorch.CBND.CoreMod.Items.Tools
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentLightInSkeleton : ClientComponent
    {
        private IReadOnlyItemLightConfig lightConfig;

        private BaseClientComponentLightSource lightSource;

        private IComponentSkeleton skeletonRenderer;

        private string skeletonSlotName;

        public ClientComponentLightInSkeleton()
            : base(isLateUpdateEnabled: false)
        {
        }

        public BaseClientComponentLightSource LightSource => this.lightSource;

        public void Setup(
            IComponentSkeleton skeletonRenderer,
            IReadOnlyItemLightConfig lightConfig,
            BaseClientComponentLightSource lightSource,
            string skeletonSlotName)
        {
            this.skeletonSlotName = skeletonSlotName;

            this.lightConfig = lightConfig;
            this.skeletonRenderer = skeletonRenderer;
            this.lightSource = lightSource;

            // Don't use Update(0) here as the skeleton cannot be ready now:
            // the new sprites likely to be added to it after this call.
            this.lightSource.IsEnabled = false;
        }

        // Attention: we should not use LateUpdate here.
        // When we've used it before, it caused flickering on the light in the center of the screen.
        // Probably related to the incorrect skeleton position resolve for the first call of LateUpdate
        // with a new skeleton renderer.
        // Anyway, using Update is not worse as no light delay seems to be noticeable.
        public override void Update(double deltaTime)
        {
            if (!this.skeletonRenderer.IsReady)
            {
                this.lightSource.IsEnabled = false;
                return;
            }

            // calculate sprite position offset
            var weaponSlotScreenOffset = this.skeletonRenderer.GetSlotScreenOffset(
                this.skeletonSlotName);

            var screenOffset = this.lightConfig.ScreenOffset;
            var slotWorldPosition = this.skeletonRenderer.TransformSlotPosition(
                this.skeletonSlotName,
                weaponSlotScreenOffset + (Vector2F)screenOffset,
                out _);

            var lightDrawPosition = slotWorldPosition - this.SceneObject.Position + this.lightConfig.WorldOffset;
            this.lightSource.PositionOffset = lightDrawPosition;
            this.lightSource.IsEnabled = true;
        }
    }
}