namespace AtomicTorch.CBND.CoreMod.Items.Fishing.Base
{
    using System.Collections.Generic;
    using AtomicTorch.GameEngine.Common.Primitives;

    public struct FishingBaitWeightReadOnlyList
    {
        public FishingBaitWeightReadOnlyList(IReadOnlyList<ValueWithWeight<IProtoItemFishingBait>> entries)
        {
            this.Entries = entries;
        }

        public IReadOnlyList<ValueWithWeight<IProtoItemFishingBait>> Entries { get; }

        public double GetWeightForBait(IProtoItemFishingBait protoItemFishingBait)
        {
            foreach (var entry in this.Entries)
            {
                if (ReferenceEquals(entry.Value, protoItemFishingBait))
                {
                    return entry.Weight;
                }
            }

            // not found
            return 0;
        }
    }
}