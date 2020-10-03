namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ClientComponentDisplayCaseSlotRenderer : ClientComponent
    {
        private IClientItemsContainer itemsContainer;

        private IComponentSpriteRenderer rendererShowcaseItem;

        private byte slotIndex;

        public IComponentSpriteRenderer RendererShowcaseItem => this.rendererShowcaseItem;

        public void Refresh()
        {
            var showcasedItem = this.itemsContainer.GetItemAtSlot(this.slotIndex);
            this.rendererShowcaseItem.TextureResource = showcasedItem?.ProtoItem.GroundIcon;
            var isEnabled = showcasedItem is not null;
            if (isEnabled)
            {
                this.rendererShowcaseItem.Scale = 0.333 * showcasedItem.ProtoItem.GroundIconScale;
            }

            this.rendererShowcaseItem.IsEnabled = isEnabled;
        }

        public void Setup(IItemsContainer itemsContainer, byte slotIndex)
        {
            this.itemsContainer = (IClientItemsContainer)itemsContainer;
            this.slotIndex = slotIndex;

            this.rendererShowcaseItem = Client.Rendering.CreateSpriteRenderer(
                this.SceneObject,
                TextureResource.NoTexture,
                spritePivotPoint: (0.5, 0.5));

            this.itemsContainer.StateHashChanged += this.Refresh;
        }

        protected override void OnDisable()
        {
            this.rendererShowcaseItem?.Destroy();
            this.rendererShowcaseItem = null;

            if (this.itemsContainer is null)
            {
                return;
            }

            this.itemsContainer.StateHashChanged -= this.Refresh;
            this.itemsContainer = null;
        }
    }
}