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
            : base(isLateUpdateEnabled: true)
        {
        }

        public override void LateUpdate(double deltaTime)
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

            this.LateUpdate(0);
        }
    }
}