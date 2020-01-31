namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.Structures;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientLandClaimGroupRenderer
    {
        public static readonly TextureResource TextureResourceLandClaimAreaCell
            = new TextureResource("FX/LandClaimAreaCell",
                                  qualityOffset: -100);

        private readonly List<ILogicObject> areas
            = new List<ILogicObject>();

        private readonly ILogicObject areasGroup;

        private readonly Func<IComponentSpriteRenderer> callbackGetRendererFromCache;

        private readonly Action<IComponentSpriteRenderer> callbackReturnRendererToCache;

        private readonly bool isGraceAreaRenderer;

        private readonly RenderingMaterial material;

        private readonly List<IComponentSpriteRenderer> renderers
            = new List<IComponentSpriteRenderer>();

        private RectangleInt? cachedGroupBounds;

        private bool isDisplayOverlays;

        private bool isVisible;

        public ClientLandClaimGroupRenderer(
            ILogicObject areasGroup,
            RenderingMaterial material,
            bool isGraceAreaRenderer,
            Func<IComponentSpriteRenderer> callbackGetRendererFromCache,
            Action<IComponentSpriteRenderer> callbackReturnRendererToCache)
        {
            this.areasGroup = areasGroup;
            this.material = material;
            this.isGraceAreaRenderer = isGraceAreaRenderer;
            this.callbackGetRendererFromCache = callbackGetRendererFromCache;
            this.callbackReturnRendererToCache = callbackReturnRendererToCache;
        }

        public ILogicObject AreasGroup => this.areasGroup;

        public RectangleInt Bounds
        {
            get
            {
                if (this.cachedGroupBounds.HasValue)
                {
                    return this.cachedGroupBounds.Value;
                }

                this.cachedGroupBounds = this.CalculateBounds();
                return this.cachedGroupBounds.Value;
            }
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
                if (!this.isVisible)
                {
                    return;
                }

                // recreate renderers
                this.DestroyRenderers();
                this.CreateRenderers();
            }
        }

        public bool IsEmpty => this.areas.Count == 0;

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

                if (this.isVisible)
                {
                    this.CreateRenderers();
                }
                else
                {
                    this.DestroyRenderers();
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

        public bool ContainsArea(ILogicObject area)
        {
            return this.areas.Contains(area);
        }

        public void RegisterArea(ILogicObject area)
        {
            this.areas.Add(area);
            this.cachedGroupBounds = null;
            this.DestroyRenderers();
        }

        public void UnregisterArea(ILogicObject area)
        {
            this.areas.Remove(area);
            this.cachedGroupBounds = null;
            this.DestroyRenderers();
        }

        private RectangleInt CalculateBounds()
        {
            return LandClaimSystem.SharedGetLandClaimGroupsBoundingArea(this.areasGroup,
                                                                        addGraceAreaPadding: true);
        }

        private void CreateRenderers()
        {
            this.DestroyRenderers();

            this.isVisible = true;

            if (this.isDisplayOverlays
                || this.areas.Count == 1)
            {
                // render each area as a separate single layer
                foreach (var area in this.areas)
                {
                    CreateSeparateAreaRenderer(area);
                }
            }
            else // unified layers mode
            {
                CreateUnifiedRenderers();
            }

            void CreateSeparateAreaRenderer(ILogicObject area)
            {
                var publicState = LandClaimArea.GetPublicState(area);
                var position = publicState.LandClaimCenterTilePosition;
                var protoObjectLandClaim = publicState.ProtoObjectLandClaim;
                int size = protoObjectLandClaim.LandClaimSize;
                if (this.isGraceAreaRenderer)
                {
                    size += 2 * protoObjectLandClaim.LandClaimGraceAreaPaddingSizeOneDirection;
                }

                position = new Vector2Ushort((ushort)Math.Max(0, position.X - size / 2),
                                             (ushort)Math.Max(0, position.Y - size / 2));

                var renderer = this.callbackGetRendererFromCache();
                renderer.RenderingMaterial = this.material;
                renderer.Scale = size;
                renderer.PositionOffset = position.ToVector2D();
                this.renderers.Add(renderer);
                renderer.IsEnabled = true;
            }

            void CreateUnifiedRenderers()
            {
                // create and fill quad tree with all areas
                var groupBounds = this.Bounds;
                var quadTree = QuadTreeNodeFactory.Create(
                    position: new Vector2Ushort((ushort)groupBounds.X,
                                                (ushort)groupBounds.Y),
                    size: (ushort)Math.Max(groupBounds.Width + 2,
                                           groupBounds.Height + 2));

                foreach (var area in this.areas)
                {
                    var areaBounds = LandClaimSystem.SharedGetLandClaimAreaBounds(area);
                    if (this.isGraceAreaRenderer)
                    {
                        var publicState = LandClaimArea.GetPublicState(area);
                        areaBounds = areaBounds.Inflate(publicState.ProtoObjectLandClaim.LandClaimGraceAreaPaddingSizeOneDirection);
                    }

                    var areaBoundsRight = (ushort)areaBounds.Right;
                    var areaBoundsTop = (ushort)areaBounds.Top;
                    for (var x = (ushort)areaBounds.Left; x < areaBoundsRight; x++)
                    for (var y = (ushort)areaBounds.Bottom; y < areaBoundsTop; y++)
                    {
                        quadTree.SetFilledPosition(new Vector2Ushort(x, y));
                    }
                }

                // create renderers for filled quad tree nodes
                foreach (var node in quadTree.EnumerateFilledNodes())
                {
                    var renderer = this.callbackGetRendererFromCache();
                    renderer.RenderingMaterial = this.material;

                    // uncomment to visualize quad tree
                    //var mat = CreateRenderingMaterial();
                    //mat.EffectParameters.Set("Color",
                    //                         Color.FromArgb(0xFF,
                    //                                        (byte)RandomHelper.Next(0, 255),
                    //                                        (byte)RandomHelper.Next(0, 255),
                    //                                        (byte)RandomHelper.Next(0, 255)));
                    //renderer.RenderingMaterial = mat;

                    renderer.Scale = 1 << node.SizePowerOfTwo;
                    renderer.PositionOffset = node.Position.ToVector2D();
                    this.renderers.Add(renderer);
                    renderer.IsEnabled = true;
                }

                //Api.Logger.Dev("Preparing areas group for rendering is done! Used renderers number: " + this.renderers.Count);
            }
        }

        private void DestroyRenderers()
        {
            foreach (var renderer in this.renderers)
            {
                this.callbackReturnRendererToCache(renderer);
            }

            this.renderers.Clear();

            this.isVisible = false;
        }
    }
}