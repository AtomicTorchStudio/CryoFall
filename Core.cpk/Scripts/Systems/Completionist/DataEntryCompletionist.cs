namespace AtomicTorch.CBND.CoreMod.Systems.Completionist
{
    using System;
    using AtomicTorch.CBND.GameApi.Data;

    [Serializable]
    public readonly struct DataEntryCompletionist
    {
        public DataEntryCompletionist(bool isRewardClaimed, IProtoEntity prototype)
        {
            this.IsRewardClaimed = isRewardClaimed;
            this.Prototype = prototype;
        }

        public bool IsRewardClaimed { get; }

        public IProtoEntity Prototype { get; }
    }
}