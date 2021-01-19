namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientLandClaimGroupsRendererManager : IDisposable
    {
        private static readonly IRenderingClientService RenderingService = Api.Client.Rendering;

        private readonly Stack<IComponentSpriteRenderer> cacheRenderers
            = new(capacity: 512); // initial capacity is a bit large as we need a lot of renderers

        private readonly Func<IComponentSpriteRenderer> callbackGetRendererFromCache;

        private readonly Action<IComponentSpriteRenderer> callbackReturnRendererToCache;

        private readonly DrawOrder drawOrder;

        private readonly bool isGraceAreaRenderer;

        private readonly RenderingMaterial material;

        private readonly IClientSceneObject sceneObject;

        private ClientLandClaimGroupRenderer groupRenderer;

        private bool isDisplayOverlays;

        private bool isEnabled;

        public ClientLandClaimGroupsRendererManager(
            Color zoneColor,
            DrawOrder drawOrder,
            bool isGraceAreaRenderer = false,
            bool isFlippedTexture = false)
        {
            this.drawOrder = drawOrder;
            this.isGraceAreaRenderer = isGraceAreaRenderer;

            this.material = ClientLandClaimGroupRenderer.CreateRenderingMaterial();
            this.material.EffectParameters.Set("Color",     zoneColor);
            this.material.EffectParameters.Set("IsFlipped", isFlippedTexture);

            this.sceneObject = Api.Client.Scene.CreateSceneObject(nameof(ClientLandClaimGroupsRendererManager)
                                                                  + " "
                                                                  + zoneColor);

            this.callbackGetRendererFromCache = this.GetRendererFromCache;
            this.callbackReturnRendererToCache = this.ReturnRendererToCache;

            this.UpdateByTimer();
        }

        public bool IsDisplayOverlays
        {
            get => this.isDisplayOverlays;
            set
            {
                if (this.isDisplayOverlays == value)
                {
                    return;
                }

                this.isDisplayOverlays = value;

                if (this.isEnabled)
                {
                    this.ForceUpdate();
                }
            }
        }

        public bool IsEnabled
        {
            get => this.isEnabled;
            set
            {
                if (this.isEnabled == value)
                {
                    return;
                }

                this.isEnabled = value;

                if (this.isEnabled)
                {
                    this.ForceUpdate();
                    return;
                }

                if (this.groupRenderer is null)
                {
                    return;
                }

                this.groupRenderer.IsDisplayOverlays = false;
                this.groupRenderer.IsVisible = false;
            }
        }

        public void Dispose()
        {
            this.Reset();
            this.sceneObject.Destroy();
        }

        public void RegisterArea(ILogicObject area)
        {
            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(area);
            if (areasGroup is null)
            {
                // incorrect area (probably LandClaimAreasGroup was set to null but will be set to non-null soon)
                return;
            }

            this.EnsureGroupRendererCreated();
            this.groupRenderer.RegisterArea(area);
            this.ForceUpdate();
        }

        public void RegisterBlueprint(
            Vector2Ushort tilePosition,
            IProtoObjectLandClaim protoObjectLandClaim)
        {
            this.EnsureGroupRendererCreated();
            this.groupRenderer.RegisterBlueprint(tilePosition, protoObjectLandClaim);
            this.ForceUpdate();
        }

        public void Reset()
        {
            if (this.groupRenderer is null)
            {
                return;
            }

            this.groupRenderer.IsVisible = false;
            this.groupRenderer = null;
        }

        public void UnregisterArea(ILogicObject area)
        {
            if (this.groupRenderer is null)
            {
                return;
            }

            this.groupRenderer.UnregisterArea(area);
            if (this.groupRenderer.IsEmpty)
            {
                this.groupRenderer.IsVisible = false;
                this.groupRenderer = null;
            }

            this.ForceUpdate();
        }

        private static bool IsWithinVisibilityBounds(
            in RectangleInt areasGroupBounds,
            in RectangleInt cameraBounds)
        {
            return areasGroupBounds.Intersects(cameraBounds);
        }

        private void EnsureGroupRendererCreated()
        {
            this.groupRenderer ??= new ClientLandClaimGroupRenderer(this.material,
                                                                    this.isGraceAreaRenderer,
                                                                    this.callbackGetRendererFromCache,
                                                                    this.callbackReturnRendererToCache);
        }

        private void ForceUpdate()
        {
            if (!this.isEnabled
                || this.groupRenderer is null)
            {
                return;
            }

            var cameraBounds = GetCameraBounds();
            this.groupRenderer.IsDisplayOverlays = this.isDisplayOverlays;
            this.groupRenderer.IsVisible = IsWithinVisibilityBounds(areasGroupBounds: this.groupRenderer.Bounds,
                                                                    cameraBounds);

            static RectangleInt GetCameraBounds()
            {
                var bounds = Api.Client.Rendering.WorldCameraCurrentViewWorldBounds;
                return new RectangleInt((int)bounds.MinX,
                                        (int)bounds.MinY,
                                        (int)bounds.Size.X + 2,
                                        (int)bounds.Size.Y + 2);
            }
        }

        private IComponentSpriteRenderer GetRendererFromCache()
        {
            if (this.cacheRenderers.Count > 0)
            {
                return this.cacheRenderers.Pop();
            }

            var renderer = RenderingService.CreateSpriteRenderer(
                this.sceneObject,
                textureResource: null,
                drawOrder: this.drawOrder);

            renderer.SortByWorldPosition = false;
            return renderer;
        }

        private void ReturnRendererToCache(IComponentSpriteRenderer spriteRenderer)
        {
            spriteRenderer.IsEnabled = false;
            this.cacheRenderers.Push(spriteRenderer);
        }

        private void UpdateByTimer()
        {
            // schedule next update
            ClientTimersSystem.AddAction(delaySeconds: 0.5,
                                         this.UpdateByTimer);

            this.ForceUpdate();
        }

        public void UnregisterBlueprint()
        {
            this.groupRenderer?.UnregisterBlueprint();
            this.ForceUpdate();
        }
    }
}