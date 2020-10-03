namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering
{
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentWorldObjectVibration : ClientComponent
    {
        private static readonly EffectResource EffectResource = new EffectResource("Special/Vibration");

        private RenderingMaterial material;

        private RenderingMaterial originalMaterial;

        private Vector2D originalPositionOffset;

        private IComponentSpriteRenderer spriteRenderer;

        public void Setup(
            IComponentSpriteRenderer spriteRenderer,
            double amplitude,
            double speed,
            double verticalStartOffsetRelative)
        {
            this.spriteRenderer = spriteRenderer;
            this.originalPositionOffset = spriteRenderer.PositionOffset;
            this.originalMaterial = spriteRenderer.RenderingMaterial;
            Client.Rendering.PreloadEffectAsync(EffectResource);

            this.material = RenderingMaterial.Create(EffectResource);
            this.material.EffectParameters
                .Set("Amplitude",                   (float)amplitude)
                .Set("Speed",                       (float)speed)
                .Set("VerticalStartOffsetRelative", (float)verticalStartOffsetRelative);

            this.RefreshMaterial();
        }

        protected override void OnDisable()
        {
            this.RefreshMaterial();
        }

        protected override void OnEnable()
        {
            this.RefreshMaterial();
        }

        private void RefreshMaterial()
        {
            if (this.spriteRenderer is null)
            {
                return;
            }

            this.spriteRenderer.RenderingMaterial = this.IsEnabled
                                                        ? this.material
                                                        : this.originalMaterial;
        }
    }
}