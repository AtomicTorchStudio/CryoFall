namespace AtomicTorch.CBND.CoreMod.Items
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemWithSlotOverlay : IProtoItem
    {
        void ClientCreateItemSlotOverlayControls(IItem item, List<Control> controls);
    }
}