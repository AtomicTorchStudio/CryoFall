namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ComponentGeneratorSolarPanelsRenderer : ClientComponent
    {
        private static readonly IRenderingClientService Rendering = Api.Client.Rendering;

        private readonly Dictionary<IItem, IComponentSpriteRenderer> slotRenderers
            = new Dictionary<IItem, IComponentSpriteRenderer>();

        private double baseDrawOrderOffsetY;

        private IClientItemsContainer itemsContainer;

        public void Setup(IItemsContainer setItemsContainer, double baseDrawOrderOffsetY)
        {
            if (this.itemsContainer is not null)
            {
                throw new Exception("Items container is already assigned");
            }

            this.itemsContainer = (IClientItemsContainer)setItemsContainer;
            this.baseDrawOrderOffsetY = baseDrawOrderOffsetY;
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

        private static Vector2D GetDrawPixelsOffset(IItem item)
        {
            return item.ContainerSlotId switch
            {
                0 => (254, 352), // top-left
                1 => (430, 285), // top-right
                2 => (254, 196), // bottom-left
                3 => (430, 129), // bottom-right
                _ => (256, 256)  // not supported
            };
        }

        private void CreateSpriteRenderer(IItem item)
        {
            var texture = (item.ProtoGameObject as IProtoItemSolarPanel)?.ObjectSprite
                          ?? ItemSolarPanelBroken.ObjectSpriteBroken;

            var spriteRenderer = Rendering.CreateSpriteRenderer(
                this.SceneObject,
                texture,
                drawOrder: DrawOrder.Default);

            spriteRenderer.PositionOffset = GetDrawPixelsOffset(item) / 256.0;
            spriteRenderer.SpritePivotPoint = (1, 0);
            spriteRenderer.DrawOrderOffsetY = this.baseDrawOrderOffsetY - spriteRenderer.PositionOffset.Y;

            this.slotRenderers.Add(item, spriteRenderer);
        }

        private void DestroyAllRenderers()
        {
            foreach (var componentSpriteRenderer in this.slotRenderers)
            {
                componentSpriteRenderer.Value.Destroy();
            }

            this.slotRenderers.Clear();
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
            this.CreateSpriteRenderer(item);
        }

        private void ItemRemovedHandler(IItem item, byte slotId)
        {
            // an item was removed - remove the renderer for this item slot
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

            foreach (var item in this.itemsContainer.Items)
            {
                this.CreateSpriteRenderer(item);
            }
        }
    }
}