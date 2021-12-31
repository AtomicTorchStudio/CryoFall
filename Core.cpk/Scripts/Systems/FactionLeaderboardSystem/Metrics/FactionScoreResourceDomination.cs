namespace AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem.Metrics
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// (PvP-only) This metric provides score points for each captured deposit (the total value is accumulated).
    /// </summary>
    public class FactionScoreResourceDomination : ProtoFactionScoreMetric
    {
        private const double ScorePerControlledDepositPerMetricRefresh = 3.0;

        public override string Description =>
            "Awarded for captured oil and lithium deposits every hour (the land claim must be transferred to the faction ownership).";

        public override double FinalScoreCoefficient => 1 / FactionLeaderboardSystem.TotalScoreMultiplier;

        public override string Name => "Resource domination score";

        public override double PowerCoefficient => 1.0;

        public override double ServerGetCurrentValue(ILogicObject faction)
        {
            if (PveSystem.ServerIsPvE)
            {
                return 0;
            }

            // calculate the current number of the controlled deposits 
            var controlledDepositsNumber = 0;
            var clanTag = FactionSystem.SharedGetClanTag(faction);

            using var tempList = Api.Shared.GetTempList<IStaticWorldObject>();

            foreach (var protoObjectDeposit in Api.FindProtoEntities<IProtoObjectDeposit>())
            {
                tempList.Clear();
                protoObjectDeposit.GetAllGameObjects(tempList.AsList());
                foreach (var objDeposit in tempList.AsList())
                {
                    if (LandClaimSystem.SharedIsWorldObjectOwnedByFaction(objDeposit,
                                                                          out var ownerClanTag)
                        && ownerClanTag == clanTag)
                    {
                        // found a deposit owned by the faction
                        controlledDepositsNumber++;
                    }
                }
            }

            var factionPrivateState = Faction.GetPrivateState(faction);
            if (controlledDepositsNumber > 0)
            {
                // increase the accumulated score
                // (the metric refresh happens together with leaderboard update once an hour)
                factionPrivateState.ServerMetricResourceDominationScore
                    += controlledDepositsNumber * ScorePerControlledDepositPerMetricRefresh;
            }

            return factionPrivateState.ServerMetricResourceDominationScore;
        }

        public override void ServerInitialize()
        {
        }
    }
}