namespace AtomicTorch.CBND.CoreMod.RatesPresets
{
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.RatesPresets.Base;

    public class RatesPresetLocalServerNormal : BaseRatesPreset
    {
        public override string Description =>
            "Standard game rules with balanced difficulty, suitable for solo experience on a local server. If this is your first time playing CryoFall—we recommend using this set of rules.";

        public override bool IsMultiplayerOnly => false;

        public override string Name => "Survival";

        public override BaseRatesPreset OrderAfterPreset
            => this.GetPreset<RatesPresetLocalServerEasy>();

        protected override void PreparePreset(RatesPreset rates)
        {
            // basic
            rates.Set<RateStructuresDecayEnabled, bool>(false);
            rates.Set<RateAdditionalLandClaimsNumber, byte>(10);
            rates.Set<RateIsTradeChatRoomEnabled, bool>(false);
            rates.Set<RateFactionMembersMaxPublicFaction, ushort>(0); // disable public factions
            rates.Set<RateFactionCreateCost, ushort>(10);             // cheap private faction cost
            rates.Set<RatePvPTimeGates, string>(RatePvPTimeGates.ValueNoTimeGates);

            // progression
            rates.Set<RateLearningPointsGainMultiplier, double>(2.0);
            rates.Set<RateSkillExperienceGainMultiplier, double>(2.0);
            rates.Set<RateCraftingSpeedMultiplier, double>(2.0);
            rates.Set<RateTimeDependentGeneratorsRate, double>(3.0);

            // difficulty
            // no changes

            // events
            rates.Set<RateBossDifficultyPragmiumQueen, double>(1.0);       // must be max 1.0 for solo
            rates.Set<RateBossDifficultySandTyrant, double>(1.0);          // must be max 1.0 for solo
            rates.Set<RateWorldEventInitialDelayMultiplier, double>(0.05); // all events can start almost immediately
            rates.Set<RateWorldEventIntervalBossPragmiumQueen, string>("3.0-5.0");
            rates.Set<RateWorldEventIntervalBossSandTyrant, string>("3.0-5.0");

            // food/farming
            rates.Set<RateItemFoodAndConsumablesShelfLifeMultiplier, double>(0.5);
            rates.Set<RateFarmPlantsLifetimeMultiplier, double>(0.5);
            rates.Set<RateFarmPlantsGrowthSpeedMultiplier, double>(2.0);
        }
    }
}