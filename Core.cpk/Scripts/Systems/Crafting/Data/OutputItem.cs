namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class OutputItem : ProtoItemWithCount
    {
        public readonly ushort CountRandom;

        public readonly double Probability;

        public OutputItem(
            IProtoItem protoItem,
            ushort count,
            ushort countRandom,
            double probability)
            : base(protoItem,
                   count)
        {
            if (probability <= 0
                || probability > 1)
            {
                throw new Exception("Output item probability must be > 0 and <= 1 for " + protoItem);
            }

            this.CountRandom = countRandom;
            this.Probability = probability;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1}: {2}, {3}: {4}",
                                 base.ToString(),
                                 nameof(this.CountRandom),
                                 this.CountRandom,
                                 nameof(this.Probability),
                                 this.Probability);
        }
    }
}