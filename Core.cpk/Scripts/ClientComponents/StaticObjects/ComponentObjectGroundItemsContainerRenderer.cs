namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public class ComponentObjectGroundItemsContainerRenderer : ClientComponent
    {
        private const float ItemOnGroundScaleMultiplier = 0.33f;

        private const float TextureObjectSackScale = 0.5f;

        private const double TextureResourceItemIconShadowScaleMultiplier = 2.0;

        private const byte TextureResourceItemIconShadowOpacity = 0xCC; // a hex number, 0xFF max 

        private static readonly IRenderingClientService Rendering = Api.Client.Rendering;

        private static readonly ITextureResource TextureResourceItemIconShadow
            = new TextureResource("FX/GroundItemShadow",
                                  qualityOffset: -100);

        private readonly Dictionary<IItem, SlotRenderer> slotRenderers
            = new();

        private bool isObjectSackMode;

        private IClientItemsContainer itemsContainer;

        private IComponentSpriteRenderer spriteRenderObjectSack;

        public void Setup(IItemsContainer setItemsContainer)
        {
            if (this.itemsContainer is not null)
            {
                throw new Exception("Items container is already assigned");
            }

            this.itemsContainer = (IClientItemsContainer)setItemsContainer;
            this.EventsSubscribe();
            this.RebuildAll();
        }

        public override void Update(double deltaTime)
        {
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.DestroyAllRenderers();
            this.EventsUnsubscribe();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            this.EventsSubscribe();
            this.RebuildAll();
        }

        private static Vector2D GetDrawOffset(IItem item)
        {
            var allItems = item.Container.Items;
            var virtualSlotId = 0;
            // let's find the slot
            foreach (var otherItem in allItems)
            {
                if (otherItem == item)
                {
                    break;
                }

                virtualSlotId++;
            }

            var occupiedSlotsCount = item.Container.OccupiedSlotsCount;
            if (occupiedSlotsCount <= 1)
            {
                // center
                return (0.5, 0.5);
            }

            if (occupiedSlotsCount <= 2)
            {
                // side-by-side two items
                switch (virtualSlotId)
                {
                    case 0:
                        return (0.25, 0.5);
                    case 1:
                        return (0.75, 0.5);
                }
            }

            if (occupiedSlotsCount <= 3)
            {
                // triangle layout
                switch (virtualSlotId)
                {
                    case 0:
                        return (0.5, 0.75);
                    case 1:
                        return (0.75, 0.25);
                    case 2:
                        return (0.25, 0.25);
                }
            }

            // quad layout (that's right, only first 4 objects of container can be drawn this way)
            return virtualSlotId switch
            {
                0 => (0.25, 0.75),
                1 => (0.75, 0.75),
                2 => (0.25, 0.25),
                3 => (0.75, 0.25),
                _ => (0.5, 0.5)
            };
        }

        private void CreateSpriteRenderer(IItem item)
        {
            var protoItem = (IProtoItem)item.ProtoGameObject;
            var icon = protoItem.GroundIcon;
            var spriteRenderer = Rendering.CreateSpriteRenderer(
                this.SceneObject,
                icon,
                // draw over floor and low shadow
                drawOrder: DrawOrder.GroundItem);

            var shadowRenderer = Client.Rendering.CreateSpriteRenderer(
                this.SceneObject,
                TextureResourceItemIconShadow,
                drawOrder: DrawOrder.GroundItem - 1);

            spriteRenderer.PositionOffset = GetDrawOffset(item);
            spriteRenderer.SpritePivotPoint = (0.5, 0.5);
            spriteRenderer.Scale = protoItem.GroundIconScale * ItemOnGroundScaleMultiplier;

            shadowRenderer.PositionOffset = spriteRenderer.PositionOffset;
            shadowRenderer.SpritePivotPoint = spriteRenderer.SpritePivotPoint;
            shadowRenderer.Scale = spriteRenderer.Scale * TextureResourceItemIconShadowScaleMultiplier;
            shadowRenderer.Color = Color.FromArgb(TextureResourceItemIconShadowOpacity, 0xFF, 0xFF, 0xFF);

            var slotRenderer = new SlotRenderer(spriteRenderer, shadowRenderer);
            this.slotRenderers.Add(item, slotRenderer);
        }

        private void DestroyAllRenderers()
        {
            this.isObjectSackMode = false;

            foreach (var componentSpriteRenderer in this.slotRenderers)
            {
                componentSpriteRenderer.Value.Destroy();
            }

            this.slotRenderers.Clear();

            this.spriteRenderObjectSack?.Destroy();
            this.spriteRenderObjectSack = null;
        }

        private void EventsSubscribe()
        {
            if (this.itemsContainer is null)
            {
                return;
            }

            this.itemsContainer.ItemAdded += this.ItemAddedHandler;
            this.itemsContainer.ItemRemoved += this.ItemRemovedHandler;
            this.itemsContainer.ItemsReset += this.ItemsResetHandler;
        }

        private void EventsUnsubscribe()
        {
            if (this.itemsContainer is null)
            {
                return;
            }

            this.itemsContainer.ItemAdded -= this.ItemAddedHandler;
            this.itemsContainer.ItemRemoved -= this.ItemRemovedHandler;
            this.itemsContainer.ItemsReset -= this.ItemsResetHandler;
        }

        private void ItemAddedHandler(IItem item)
        {
            this.RefreshIsObjectsSackMode();
            if (this.isObjectSackMode)
            {
                return;
            }

            this.CreateSpriteRenderer(item);
            this.UpdateDrawOffsets();
        }

        private void ItemRemovedHandler(IItem item, byte slotId)
        {
            var wasObjectsSackMode = this.isObjectSackMode;
            this.RefreshIsObjectsSackMode();
            if (this.isObjectSackMode)
            {
                return;
            }

            if (wasObjectsSackMode)
            {
                // sack destroyed - show items on ground
                this.RebuildAll();
                return;
            }

            // an item was removed and it was not an object sack - remove the renderer for this item slot
            if (!this.slotRenderers.TryGetValue(item, out var renderer))
            {
                // renderer is not created for the specified slot
                return;
            }

            renderer.Destroy();
            this.slotRenderers.Remove(item);
        }

        private void ItemsResetHandler()
        {
            this.RebuildAll();
        }

        private void RebuildAll()
        {
            if (this.itemsContainer is null)
            {
                return;
            }

            this.DestroyAllRenderers();

            this.RefreshIsObjectsSackMode();

            if (this.isObjectSackMode)
            {
                return;
            }

            foreach (var item in this.itemsContainer.Items.Take(4))
            {
                this.CreateSpriteRenderer(item);
            }
        }

        private void RefreshIsObjectsSackMode()
        {
            var newIsObjectSackMode = this.itemsContainer.OccupiedSlotsCount > 4;
            if (this.isObjectSackMode == newIsObjectSackMode)
            {
                return;
            }

            this.DestroyAllRenderers();
            this.isObjectSackMode = newIsObjectSackMode;

            if (!this.isObjectSackMode)
            {
                return;
            }

            this.spriteRenderObjectSack = Rendering.CreateSpriteRenderer(
                this.SceneObject,
                ObjectGroundItemsContainer.TextureResourceSack,
                drawOrder: DrawOrder.Default,
                positionOffset: (0.5, 0.5),
                spritePivotPoint: (0.5, 0.5),
                scale: TextureObjectSackScale);

            this.spriteRenderObjectSack.DrawOrderOffsetY = -0.125f;
        }

        private void UpdateDrawOffsets()
        {
            foreach (var pair in this.slotRenderers)
            {
                var drawOffset = GetDrawOffset(pair.Key);
                var slotRenderer = pair.Value;
                slotRenderer.Renderer.PositionOffset = drawOffset;

                if (slotRenderer.RendererShadow is not null)
                {
                    slotRenderer.RendererShadow.PositionOffset = drawOffset;
                }
            }
        }

        private readonly struct SlotRenderer
        {
            [NotNull]
            public readonly IComponentSpriteRenderer Renderer;

            public readonly IComponentSpriteRenderer RendererShadow;

            public SlotRenderer(IComponentSpriteRenderer renderer, IComponentSpriteRenderer rendererShadow)
            {
                this.Renderer = renderer;
                this.RendererShadow = rendererShadow;
            }

            public void Destroy()
            {
                this.Renderer.Destroy();
                this.RendererShadow?.Destroy();
            }
        }
    }
}