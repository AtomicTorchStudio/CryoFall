namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class SkillCrafting : ProtoSkill
    {
        /// <summary>
        /// Base number of crafting queue slots.
        /// This skill is adding additional slots (see below).
        /// </summary>
        public const byte BaseCraftingSlotsCount = 4;

        ///// <summary>
        ///// (not yet implemented) Exp given for crafting a new recipe for the first time.
        ///// </summary>
        //public const double ExperiencePerNewRecipeCrafted = 250.0;

        /// <summary>
        /// Exp given per time spent crafting items, this is supposed to be the main source of exp
        /// for advancing this skill as more complex items typically require more time to craft,
        /// although it is not necessarily have to be 100% representative of their difficulty.
        /// </summary>
        public const double ExperienceForCraftingPerSecond = 2.0;

        public override string Description =>
            "Crafting more and more complex items and understanding their structure is a good way to learn what makes the technology of this world tick.";

        public override double ExperienceToLearningPointsConversionMultiplier => 1; // standard speed

        public override bool IsSharingLearningPointsWithPartyMembers => true;

        public override string Name => "Crafting";

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            config.Category = GetCategory<SkillCategoryTechnical>();

            // Note: total effect would be +100% crafting speed (or -50% crafting duration) at level 20.
            config.AddStatEffect(
                StatName.CraftingSpeed,
                formulaPercentBonus: level => level * 4);

            config.AddStatEffect(
                StatName.CraftingSpeed,
                level: 10,
                percentBonus: 10);

            config.AddStatEffect(
                StatName.CraftingSpeed,
                level: 20,
                percentBonus: 10);

            // add 2 extra slots
            config.AddStatEffect(
                StatName.CraftingQueueMaxSlotsCount,
                level: 5,
                valueBonus: 1);

            config.AddStatEffect(
                StatName.CraftingQueueMaxSlotsCount,
                level: 15,
                valueBonus: 1);

            CraftingMechanics.ServerNonManufacturingRecipeCrafted += ServerOnRecipeCraftedHandler;
        }

        // Reward player for the crafted item.
        private static void ServerOnRecipeCraftedHandler(CraftingQueueItem craftingQueueItem)
        {
            if (!(craftingQueueItem.GameObject is ICharacter character)
                || character.ProtoCharacter.GetType() != typeof(PlayerCharacter))
            {
                // not crafted by player or player prototype is not exactly PlayerCharacter
                return;
            }

            // add experience according to the crafted recipe's original duration
            var exp = ExperienceForCraftingPerSecond
                      * craftingQueueItem.Recipe.OriginalDuration;
            character.ServerAddSkillExperience<SkillCrafting>(exp);
        }
    }
}