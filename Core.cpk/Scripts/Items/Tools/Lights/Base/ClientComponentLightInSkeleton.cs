namespace AtomicTorch.CBND.CoreMod.Items.Tools.Lights
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentLightInSkeleton : ClientComponent
    {
        private ICharacter character;

        private IReadOnlyItemLightConfig lightConfig;

        private BaseClientComponentLightSource lightSource;

        private string skeletonAttachmentName;

        private string skeletonBoneName;

        private IComponentSkeleton skeletonRenderer;

        public ClientComponentLightInSkeleton()
            : base(isLateUpdateEnabled: false)
        {
        }

        public BaseClientComponentLightSource LightSource => this.lightSource;

        public void Setup(
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            IReadOnlyItemLightConfig lightConfig,
            BaseClientComponentLightSource lightSource,
            string skeletonAttachmentName,
            string skeletonBoneName)
        {
            this.skeletonAttachmentName = skeletonAttachmentName;
            this.skeletonBoneName = skeletonBoneName;

            this.lightConfig = lightConfig;
            this.character = character;
            this.skeletonRenderer = skeletonRenderer;
            this.lightSource = lightSource;

            this.Update(0);
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
                this.skeletonAttachmentName);

            var screenOffset = this.lightConfig.ScreenOffset;
            var boneWorldPosition = this.skeletonRenderer.TransformBonePosition(
                this.skeletonBoneName,
                weaponSlotScreenOffset + (Vector2F)screenOffset,
                out _);

            var lightDrawPosition = boneWorldPosition - this.character.Position;
            this.lightSource.PositionOffset = lightDrawPosition;
            this.lightSource.IsEnabled = true;
        }
    }
}