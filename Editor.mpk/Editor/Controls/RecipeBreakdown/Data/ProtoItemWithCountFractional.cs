namespace AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown.Data
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ProtoItemWithCountFractional
    {
        public readonly double Count;

        public readonly IProtoItem ProtoItem;

        public ProtoItemWithCountFractional(IProtoItem protoItem, double count)
        {
            this.ProtoItem = protoItem;
            this.Count = count;
        }
    }
}