namespace AtomicTorch.CBND.CoreMod.RatesPresets
{
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.RatesPresets.Base;

    public class RatesPresetLocalServerEasy : BaseRatesPreset
    {
        public override string Description =>
            "Live it up in paradise! Explore the world at your own pace and enjoy technology progression. Great for players who just want to see what the game has to offer without excessive challenges. Though, you will still have to fight your way through!";

        public override bool IsMultiplayerOnly => false;

        public override string Name => "Paradise";

        protected override void PreparePreset(RatesPreset rates)
        {
            // basic
            rates.Set<RateStructuresDecayEnabled, bool>(false);
            rates.Set<RateAdditionalLandClaimsNumber, byte>(10);
            rates.Set<RateIsTradeChatRoomEnabled, bool>(false);
            rates.Set<RateFactionMembersMaxPublicFaction, ushort>(0); // disable public factions
            rates.Set<RateFactionCreateCost, ushort>(10);             // cheap private faction cost

            // progression
            rates.Set<RateLearningPointsGainMultiplier, double>(2.0);
            rates.Set<RateSkillExperienceGainMultiplier, double>(2.0);
            rates.Set<RateCraftingSpeedMultiplier, double>(4.0);
            rates.Set<RateActionMiningSpeedMultiplier, double>(2.0);
            rates.Set<RateActionWoodcuttingSpeedMultiplier, double>(2.0);
            rates.Set<RateTimeDependentGeneratorsRate, double>(3.0);

            // difficulty
            rates.Set<RateDamageByCreaturesMultiplier, double>(0.75);
            rates.Set<RateHunger, double>(0.8);
            rates.Set<RateThirst, double>(0.8);

            // events
            rates.Set<RateBossDifficultyPragmiumQueen, double>(0.5);
            rates.Set<RateBossDifficultySandTyrant, double>(0.5);
            rates.Set<RateWorldEventInitialDelayMultiplier, double>(0.05); // all events can start almost immediately
            rates.Set<RateWorldEventIntervalBossPragmiumQueen, string>("3.0-5.0");
            rates.Set<RateWorldEventIntervalBossSandTyrant, string>("3.0-5.0");

            // food/farming
            rates.Set<RateItemFoodAndConsumablesShelfLifeMultiplier, double>(0.5); // must be shorter for local server
            rates.Set<RateFarmPlantsLifetimeMultiplier, double>(0.5);              // must be shorter for local server
            rates.Set<RateFarmPlantsGrowthSpeedMultiplier, double>(2.0);
        }
    }
}