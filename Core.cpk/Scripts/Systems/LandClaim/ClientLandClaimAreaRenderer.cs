namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientLandClaimAreaRenderer : IDisposable
    {
        public static readonly TextureResource TextureResourceLandClaimAreaCell
            = new TextureResource("FX/LandClaimAreaCell",
                                  qualityOffset: -100);

        private static readonly IRenderingClientService RenderingService = Api.Client.Rendering;

        private readonly DrawOrder drawOrder;

        private readonly bool isGraceAreaRenderer;

        private readonly RenderingMaterial material;

        private readonly IDictionary<ILogicObject, IComponentSpriteRenderer> renderers
            = new Dictionary<ILogicObject, IComponentSpriteRenderer>();

        private readonly IClientSceneObject sceneObject;

        private bool isVisible;

        public ClientLandClaimAreaRenderer(
            Color zoneColor,
            DrawOrder drawOrder,
            bool isGraceAreaRenderer = false)
        {
            this.drawOrder = drawOrder;
            this.isGraceAreaRenderer = isGraceAreaRenderer;
            this.material = CreateRenderingMaterial();
            this.material.EffectParameters.Set("Color", zoneColor);

            this.sceneObject = Api.Client.Scene.CreateSceneObject(
                nameof(ClientLandClaimAreaRenderer) + " with color=" + zoneColor);
        }

        public bool IsVisible
        {
            get => this.isVisible;
            set
            {
                if (this.isVisible == value)
                {
                    return;
                }

                this.isVisible = value;

                foreach (var renderer in this.renderers)
                {
                    renderer.Value.IsEnabled = this.isVisible;
                }
            }
        }

        public static RenderingMaterial CreateRenderingMaterial()
        {
            var material = RenderingMaterial.Create(new EffectResource("LandClaimArea"));
            material.EffectParameters.Set("SpriteTexture",
                                          TextureResourceLandClaimAreaCell);
            return material;
        }

        public void AddRenderer(ILogicObject area)
        {
            var publicState = LandClaimArea.GetPublicState(area);
            var pos = publicState.LandClaimCenterTilePosition;
            var landClaimAreaSize = publicState.LandClaimSize;
            if (this.isGraceAreaRenderer)
            {
                landClaimAreaSize = (ushort)(landClaimAreaSize
                                             + 2 * publicState.LandClaimGraceAreaPaddingSizeOneDirection);
            }

            var position = new Vector2Ushort(
                (ushort)Math.Max(0, pos.X - landClaimAreaSize / 2),
                (ushort)Math.Max(0, pos.Y - landClaimAreaSize / 2));
            if (this.renderers.TryGetValue(area, out var renderer))
            {
                throw new Exception("Renderer already exists");
            }

            renderer = RenderingService.CreateSpriteRenderer(
                this.sceneObject,
                textureResource: null,
                drawOrder: this.drawOrder,
                positionOffset: position.ToVector2D());

            renderer.RenderingMaterial = this.material;
            renderer.SortByWorldPosition = false;
            renderer.IgnoreTextureQualityScaling = true;
            renderer.Scale = landClaimAreaSize;

            this.renderers[area] = renderer;

            renderer.IsEnabled = this.isVisible;
        }

        public void Dispose()
        {
            this.sceneObject.Destroy();
        }

        public bool RemoveRenderer(ILogicObject logicObjectArea)
        {
            if (!this.renderers.TryGetValue(logicObjectArea, out var renderer))
            {
                return false;
            }

            this.renderers.Remove(logicObjectArea);
            renderer.Destroy();
            return true;
        }

        public void Reset()
        {
            this.DestroyAllRenderers();
        }

        private void DestroyAllRenderers()
        {
            foreach (var renderer in this.renderers.Values)
            {
                renderer.Destroy();
            }

            this.renderers.Clear();
        }
    }
}