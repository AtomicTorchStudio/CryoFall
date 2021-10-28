namespace AtomicTorch.CBND.CoreMod.RatesPresets
{
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.RatesPresets.Base;

    public class RatesPresetLocalServerHardcore : BaseRatesPreset
    {
        public override string Description =>
            "Welcome to the world where everything wants to kill you. This option offers similar rules to the standard survival game mode but with additional challenges for experienced players who want higher stakes and higher rewards, especially in the end game.";

        public override bool IsMultiplayerOnly => false;

        public override string Name => "Hardcore";

        public override BaseRatesPreset OrderAfterPreset
            => this.GetPreset<RatesPresetLocalServerNormal>();

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
            rates.Set<RateDamageByCreaturesMultiplier, double>(1.5);
            rates.Set<RateHunger, double>(1.3);
            rates.Set<RateThirst, double>(1.3);

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