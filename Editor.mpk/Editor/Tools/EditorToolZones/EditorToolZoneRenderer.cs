namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolZones
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Data.Structures;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class EditorToolZoneRenderer : IDisposable
    {
        public const float ZoneOpacity = 0.5f;

        private static readonly IRenderingClientService RenderingService = Api.Client.Rendering;

        public readonly ClientZoneProvider ZoneProvider;

        private readonly RenderingMaterial material;

        private readonly IDictionary<QuadTreeNodeKey, IComponentSpriteRenderer> renderers
            = new Dictionary<QuadTreeNodeKey, IComponentSpriteRenderer>();

        private readonly IClientSceneObject sceneObject;

        private DrawOrder drawOrder;

        private RectangleInt lastCheckBounds;

        private Color zoneColor;

        // Please do not change the default value == -1
        private sbyte zoneIndex = -1;

        public EditorToolZoneRenderer(ClientZoneProvider zoneProvider)
        {
            this.material = RenderingMaterial.Create(new EffectResource("LandClaimArea"));
            this.material.EffectParameters.Set("SpriteTexture",
                                               new TextureResource("FX/EditorZoneTile"));

            this.sceneObject = Api.Client.Scene.CreateSceneObject(
                "EditorToolZones / Zone renderer - " + zoneProvider.ProtoZone.ShortId);

            this.ZoneProvider = zoneProvider;
            this.ZoneProvider.ZoneModified += this.ZoneModifiedHandler;
            this.ZoneProvider.ZoneReset += this.ZoneResetHandler;
            ClientUpdateHelper.UpdateCallback += this.Update;

            this.RebuildAllRenderers();
        }

        public Color Color => this.zoneColor;

        public sbyte ZoneIndex
        {
            get => this.zoneIndex;
            set
            {
                if (this.zoneIndex == value)
                {
                    return;
                }

                Api.Assert(value >= 0, "Zone index must be >= 0");

                this.zoneIndex = value;
                this.drawOrder = DrawOrder.Overlay - 1 - (byte)this.zoneIndex;

                //Api.Logger.WriteDev(
                //	"Zone index set: " + this.ZoneProvider.ProtoZone + " index=" + this.zoneIndex + " drawOrder=" + this.drawOrder);

                this.zoneColor = ClientZoneColors.GetZoneColor((byte)this.zoneIndex);
                this.SetZoneColor(this.zoneColor);

                foreach (var componentSpriteRenderer in this.renderers.Values)
                {
                    componentSpriteRenderer.DrawOrder = this.drawOrder;
                }
            }
        }

        public void Dispose()
        {
            this.ZoneProvider.ZoneModified -= this.ZoneModifiedHandler;
            this.ZoneProvider.ZoneReset -= this.ZoneResetHandler;
            ClientUpdateHelper.UpdateCallback -= this.Update;
            this.sceneObject.Destroy();
        }

        private static RectangleInt CalculateCurrentVisibleWorldBounds()
        {
            var visibleWorldBounds = Api.Client.Rendering.WorldCameraCurrentViewWorldBounds;
            var minX = (ushort)Math.Max(0, visibleWorldBounds.MinX - 1);
            var minY = (ushort)Math.Max(0, visibleWorldBounds.MinY - 1);
            var maxX = (ushort)Math.Min(ushort.MaxValue, visibleWorldBounds.MaxX + 1);
            var maxY = (ushort)Math.Min(ushort.MaxValue, visibleWorldBounds.MaxY + 1);
            var bounds = new RectangleInt(minX, minY, maxX - minX, maxY - minY);
            return bounds;
        }

        private void AddRenderer(Vector2Ushort position, byte sizePowerOfTwo, bool throwExceptionIfExist)
        {
            var key = new QuadTreeNodeKey(position, sizePowerOfTwo);
            if (this.renderers.TryGetValue(key, out var renderer))
            {
                if (throwExceptionIfExist)
                {
                    throw new Exception("Renderer already exists");
                }

                return;
            }

            renderer = RenderingService.CreateSpriteRenderer(
                this.sceneObject,
                textureResource: null,
                drawOrder: this.drawOrder,
                positionOffset: position.ToVector2D());

            renderer.RenderingMaterial = this.material;
            var size = 1 << sizePowerOfTwo;
            renderer.Scale = size;

            this.renderers[key] = renderer;
        }

        private void DestroyAllRenderers()
        {
            foreach (var renderer in this.renderers.Values)
            {
                renderer.Destroy();
            }

            this.renderers.Clear();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsVisibleQuadTreeNode(Vector2Ushort position, ushort size)
        {
            return this.lastCheckBounds.Intersects(
                new RectangleInt(offset: position,
                                 size: (size, size)));
        }

        private void RebuildAllRenderers()
        {
            this.DestroyAllRenderers();
            this.RefreshRenderers();
        }

        private void RefreshRenderers()
        {
            this.lastCheckBounds = CalculateCurrentVisibleWorldBounds();

            using (var listToRemove = Api.Shared.GetTempList<QuadTreeNodeKey>())
            {
                // gather and remove renderers which are not required anymore
                foreach (var pair in this.renderers)
                {
                    var node = pair.Key;
                    if (!this.IsVisibleQuadTreeNode(node.Position, node.Size))
                    {
                        listToRemove.Add(pair.Key);
                    }
                }

                foreach (var quadTreeNodeKey in listToRemove.AsList())
                {
                    this.RemoveRenderer(quadTreeNodeKey.Position, quadTreeNodeKey.SizePowerOfTwo);
                }
            }

            if (this.ZoneProvider.IsDataReceived)
            {
                CreateRendersRecursive(this.ZoneProvider.GetQuadTree());
            }

            // local func to traverse quad tree and add missing renderers
            void CreateRendersRecursive(IQuadTreeNode node)
            {
                if (node == null
                    || !this.IsVisibleQuadTreeNode(node.Position, node.Size))
                {
                    return;
                }

                if (node.IsNodeFilled)
                {
                    this.AddRenderer(node.Position, node.SizePowerOfTwo, throwExceptionIfExist: false);
                    return;
                }

                CreateRendersRecursive(node.GetSubNode(QuadTreeSubNodeIndex.BottomLeft));
                CreateRendersRecursive(node.GetSubNode(QuadTreeSubNodeIndex.BottomRight));
                CreateRendersRecursive(node.GetSubNode(QuadTreeSubNodeIndex.TopLeft));
                CreateRendersRecursive(node.GetSubNode(QuadTreeSubNodeIndex.TopRight));
            }
        }

        private void RemoveRenderer(Vector2Ushort position, byte sizePowerOfTwo)
        {
            var key = new QuadTreeNodeKey(position, sizePowerOfTwo);
            var renderer = this.renderers[key];
            renderer.Destroy();
            this.renderers.Remove(key);
        }

        private void SetZoneColor(Color zoneColor)
        {
            this.material.EffectParameters.Set(
                "Color",
                // ReSharper disable RedundantNameQualifier
                new GameEngine.Common.Primitives.Vector4(
                    // ReSharper restore RedundantNameQualifier
                    // premultiply alpha
                    ZoneOpacity * zoneColor.R / (float)byte.MaxValue,
                    ZoneOpacity * zoneColor.G / (float)byte.MaxValue,
                    ZoneOpacity * zoneColor.B / (float)byte.MaxValue,
                    ZoneOpacity));
        }

        private void Update()
        {
            var visibleWorldBounds = CalculateCurrentVisibleWorldBounds();
            if (this.lastCheckBounds.Equals(visibleWorldBounds))
            {
                return;
            }

            this.RefreshRenderers();
        }

        private void ZoneModifiedHandler(QuadTreeDiff diff)
        {
            foreach (var removedNodes in diff.NodesRemoved)
            {
                foreach (var node in removedNodes)
                {
                    if (this.IsVisibleQuadTreeNode(node.Position, node.Size))
                    {
                        this.RemoveRenderer(node.Position, node.SizePowerOfTwo);
                    }
                }
            }

            foreach (var addedNodes in diff.NodesAdded)
            {
                foreach (var node in addedNodes)
                {
                    if (this.IsVisibleQuadTreeNode(node.Position, node.Size))
                    {
                        this.AddRenderer(node.Position, node.SizePowerOfTwo, throwExceptionIfExist: true);
                    }
                }
            }
        }

        private void ZoneResetHandler()
        {
            this.RebuildAllRenderers();
        }

        private struct QuadTreeNodeKey : IEquatable<QuadTreeNodeKey>
        {
            public readonly Vector2Ushort Position;

            public readonly byte SizePowerOfTwo;

            public QuadTreeNodeKey(Vector2Ushort position, byte sizePowerOfTwo)
            {
                this.Position = position;
                this.SizePowerOfTwo = sizePowerOfTwo;
            }

            public ushort Size => (ushort)(1 << this.SizePowerOfTwo);

            public bool Equals(QuadTreeNodeKey other)
            {
                return this.Position.Equals(other.Position) && this.SizePowerOfTwo == other.SizePowerOfTwo;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is QuadTreeNodeKey key && this.Equals(key);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (this.Position.GetHashCode() * 397)
                           ^ this.SizePowerOfTwo.GetHashCode();
                }
            }
        }
    }
}