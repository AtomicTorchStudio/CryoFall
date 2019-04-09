namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class DropItem : ProtoItemWithCount
    {
        public readonly ushort CountRandom;

        public DropItem(
            IProtoItem protoItem,
            ushort count,
            ushort countRandom)
            : base(protoItem,
                   count)
        {
            this.CountRandom = countRandom;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}: {2}",
                                 base.ToString(),
                                 nameof(this.CountRandom),
                                 this.CountRandom);
        }
    }
}