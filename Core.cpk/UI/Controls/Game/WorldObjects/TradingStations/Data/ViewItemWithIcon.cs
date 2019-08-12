namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public struct ViewItemWithIcon
    {
        public ViewItemWithIcon(IProtoItem protoItem)
        {
            this.ProtoItem = protoItem;
        }

        public string Description => this.ProtoItem?.Description;

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.ProtoItem?.Icon);

        public string Name => this.ProtoItem?.Name;

        public IProtoItem ProtoItem { get; }
    }
}