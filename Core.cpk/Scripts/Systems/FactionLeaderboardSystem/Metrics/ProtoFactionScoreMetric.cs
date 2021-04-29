namespace AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem.Metrics
{
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;

    public abstract class ProtoFactionScoreMetric : ProtoEntity
    {
        public abstract string Description { get; }

        public abstract double FinalScoreCoefficient { get; }

        /// <summary>
        /// The metric value is passed through the power function before being added to the total faction score.
        /// The reason for this is that some metrics are unlimited by their nature (e.g. amount of LP/XP)
        /// and if they linearly contribute to the total faction score, it will diminish the contribution
        /// of the limited metrics (such as faction level).
        /// All unlimited metrics that could be unlimitedly grinded must provide diminishing returns.
        /// For example, if there is no power (==1.0), then 100,000 value will provide +100,000 faction score.
        /// However, with 0.9 power it will be much less: 100,000 ^ 0.9 = +31,622 faction score.
        /// The higher the metric value the more it's reduced by the power coefficient.
        /// The power must be within (0;1] range.
        /// </summary>
        public abstract double PowerCoefficient { get; }

        public abstract double ServerGetCurrentValue(ILogicObject faction);

        public abstract void ServerInitialize();
    }
}