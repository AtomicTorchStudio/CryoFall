namespace AtomicTorch.CBND.CoreMod.Systems
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class InputItems
    {
        private readonly List<ProtoItemWithCount> items = new List<ProtoItemWithCount>();

        public int Count => this.items.Count;

        public void Add<TProtoItem>(ushort count = 1)
            where TProtoItem : class, IProtoItem, new()
        {
            var protoItem = Api.GetProtoEntity<TProtoItem>();
            this.Add(protoItem, count);
        }

        public void Add(IProtoItem protoItem, ushort count = 1)
        {
            this.items.Add(new ProtoItemWithCount(protoItem, count));
        }

        public IReadOnlyList<ProtoItemWithCount> AsReadOnly()
        {
            return this.items;
        }
    }
}