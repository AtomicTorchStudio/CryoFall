namespace AtomicTorch.CBND.CoreMod.Systems.Completionist
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Fishing.Base;
    using AtomicTorch.CBND.GameApi.Data;

    [Serializable]
    public readonly struct DataEntryCompletionistFish : ICompletionistDataEntry
    {
        public DataEntryCompletionistFish(bool isRewardClaimed, IProtoEntity prototype, float maxSizeValue)
        {
            this.IsRewardClaimed = isRewardClaimed;
            this.Prototype = prototype;
            this.MaxSizeValue = maxSizeValue;
        }

        public bool IsRewardClaimed { get; }

        public float MaxLength
            => ((IProtoItemFish)this.Prototype).MaxLength * this.MaxSizeValue;

        public float MaxSizeValue { get; }

        public float MaxWeight
            => ((IProtoItemFish)this.Prototype).MaxWeight * this.MaxSizeValue;

        public IProtoEntity Prototype { get; }
    }
}