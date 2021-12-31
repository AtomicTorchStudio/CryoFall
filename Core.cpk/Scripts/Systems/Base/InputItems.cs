namespace AtomicTorch.CBND.CoreMod.Systems
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class InputItems
    {
        private readonly List<ProtoItemWithCount> items = new();

        public int Count => this.items.Count;

        public ProtoItemWithCount this[int index] => this.items[index];

        public InputItems Add<TProtoItem>(ushort count = 1)
            where TProtoItem : class, IProtoItem, new()
        {
            var protoItem = Api.GetProtoEntity<TProtoItem>();
            this.Add(protoItem, count);
            return this;
        }

        public InputItems Add(IProtoItem protoItem, ushort count = 1)
        {
            this.items.Add(new ProtoItemWithCount(protoItem, count));
            return this;
        }

        public void ApplyRates(byte multiplier)
        {
            if (multiplier == 1)
            {
                return;
            }

            if (multiplier < 1)
            {
                throw new Exception("Rate modifier cannot be < 1");
            }

            for (var index = 0; index < this.items.Count; index++)
            {
                var entry = this.items[index];
                this.items[index] = entry.WithRate(multiplier);
            }
        }

        public IReadOnlyList<ProtoItemWithCount> AsReadOnly()
        {
            return this.items;
        }

        public InputItems Clear()
        {
            this.items.Clear();
            return this;
        }
    }
}