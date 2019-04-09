namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.AmbientOcclusion
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientAmbientOcclusion
    {
        public const double BlurDistanceHorizontal = 10;

        public const double BlurDistanceVertical = 2.5;

        /// <summary>
        /// With this constant you can enable current occlusion mask rendering.
        /// </summary>
        public const bool IsDisplayMask = false;

        /// <summary>
        /// (If you enabled IsDisplayMask) With this parameter you can enable/disable blur post-effect for the mask.
        /// </summary>
        public const bool IsDisplayMaskWithBlur = true;

        public const string RenderingTag = nameof(ClientAmbientOcclusion) + "LayerOcclusion";

        private static readonly EffectResource EffectResourceOcclusionMaskFromColorDrawEffect
            = new EffectResource("AmbientOcclusion/OcclusionMaskFromColorDrawEffect");

        public static readonly Lazy<RenderingMaterial> OcclusionSpriteFromColorRenderingMaterial
            = new Lazy<RenderingMaterial>(
                () => RenderingMaterial.Create(EffectResourceOcclusionMaskFromColorDrawEffect));

        private static readonly EffectResource EffectResourceOcclusionMaskDrawEffect
            = new EffectResource("AmbientOcclusion/OcclusionMaskDrawEffect");

        public static readonly Lazy<RenderingMaterial> OcclusionSpriteRenderingMaterial
            = new Lazy<RenderingMaterial>(() => RenderingMaterial.Create(EffectResourceOcclusionMaskDrawEffect));

        private static readonly IRenderingClientService Rendering = Api.Client.Rendering;

        public static IComponentSpriteRenderer CreateOcclusionRenderer(
            IClientSceneObject sceneObject,
            ITextureResource textureResource,
            Vector2D? positionOffset = null,
            bool extractMaskFromColor = false)
        {
            var result = Rendering.CreateSpriteRenderer(
                sceneObject,
                textureResource,
                drawOrder: DrawOrder.Occlusion,
                positionOffset: positionOffset,
                renderingTag: RenderingTag);
            SetupComponent(result, extractMaskFromColor);
            return result;
        }

        public static IComponentSpriteRenderer CreateOcclusionRenderer(
            IWorldObject worldObject,
            ITextureResource textureResource,
            Vector2D? positionOffset = null,
            bool extractMaskFromColor = false)
        {
            var result = Rendering.CreateSpriteRenderer(
                worldObject,
                textureResource,
                drawOrder: DrawOrder.Occlusion,
                positionOffset: positionOffset,
                renderingTag: RenderingTag);
            SetupComponent(result, extractMaskFromColor);
            return result;
        }

        private static void SetupComponent(IComponentSpriteRenderer result, bool extractMaskFromColor)
        {
            result.RenderingMaterial = !extractMaskFromColor
                                           ? OcclusionSpriteRenderingMaterial.Value
                                           : OcclusionSpriteFromColorRenderingMaterial.Value;
        }
    }
}