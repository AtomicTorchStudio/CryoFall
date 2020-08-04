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

        /// <summary>
        /// Max (record) length (in cm).
        /// </summary>
        public float MaxLength
            => ((IProtoItemFish)this.Prototype).MaxLength * this.MaxSizeValue;

        /// <summary>
        /// Max (record) size value of the fish the player has caught (in the range from 0 to 1).
        /// </summary>
        public float MaxSizeValue { get; }

        /// <summary>
        /// Max (record) weight (in kg).
        /// </summary>
        public float MaxWeight
        {
            get
            {
                var maxWeight = ((IProtoItemFish)this.Prototype).MaxWeight;
                double size = this.MaxSizeValue;

                // the fish weight doesn't scale linearly with its length
                size = Math.Pow(size, 1.5);

                return (float)(size * maxWeight);
            }
        }

        public IProtoEntity Prototype { get; }
    }
}