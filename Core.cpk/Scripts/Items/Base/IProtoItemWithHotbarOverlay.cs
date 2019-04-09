namespace AtomicTorch.CBND.CoreMod.Items
{
    using System.Windows.Controls;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemWithHotbarOverlay : IProtoItem
    {
        Control ClientCreateHotbarOverlayControl(IItem item);
    }
}