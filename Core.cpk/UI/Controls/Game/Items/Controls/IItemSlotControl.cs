namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls
{
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IItemSlotControl : ICacheableControl
    {
        void RefreshItem();

        void Setup(IItemsContainer setContainer, byte slotId);
    }
}