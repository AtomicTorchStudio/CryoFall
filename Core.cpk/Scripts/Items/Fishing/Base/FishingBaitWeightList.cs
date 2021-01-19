namespace AtomicTorch.CBND.CoreMod.Items.Fishing.Base
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class FishingBaitWeightList
    {
        private readonly List<ValueWithWeight<IProtoItemFishingBait>> entries
            = new();

        public FishingBaitWeightList Add<TProtoItemFishingBait>(double weight)
            where TProtoItemFishingBait : class, IProtoItemFishingBait, new()
        {
            var protoItem = Api.GetProtoEntity<TProtoItemFishingBait>();
            return this.Add(protoItem, weight);
        }

        public FishingBaitWeightList Add(IProtoItemFishingBait protoItemBait, double weight)
        {
            this.entries.Add(new ValueWithWeight<IProtoItemFishingBait>(protoItemBait, weight));
            return this;
        }

        public FishingBaitWeightReadOnlyList ToReadOnly()
        {
            return new(this.entries.ToArray());
        }
    }
}