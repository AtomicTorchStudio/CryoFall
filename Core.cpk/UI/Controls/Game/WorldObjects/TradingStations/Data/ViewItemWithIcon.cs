namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.TradingStations.Data
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public readonly struct ViewItemWithIcon : IEquatable<ViewItemWithIcon>
    {
        public ViewItemWithIcon(IProtoItem protoItem)
        {
            this.ProtoItem = protoItem;
        }

        public string Description => this.ProtoItem?.Description;

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.ProtoItem?.Icon);

        public string Name => this.ProtoItem?.Name;

        public IProtoItem ProtoItem { get; }

        public bool Equals(ViewItemWithIcon other)
        {
            return Equals(this.ProtoItem, other.ProtoItem);
        }

        public override bool Equals(object obj)
        {
            return obj is ViewItemWithIcon other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return this.ProtoItem != null ? this.ProtoItem.GetHashCode() : 0;
        }
    }
}