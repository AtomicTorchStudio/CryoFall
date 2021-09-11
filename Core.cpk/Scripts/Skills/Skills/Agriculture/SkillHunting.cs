namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class SkillHunting : ProtoSkill //<SkillHunting.Flags>
    {
        /// <summary>
        /// Hunting isn't that easy, so decent amount of experience.
        /// Please note that this value is multiplied with ProtoCharacterMob.MobKillExperienceMultiplier to get final value of exp.
        /// </summary>
        public const double ExperienceForKill = 250;

        /// <summary>
        /// Flat amount of experience for looting a body.
        /// </summary>
        public static double ExperienceForGather => 50;

        public override string Description =>
            "Being a better hunter increases your chances to get more spoils from each game.";

        /// <summary>
        /// Less LP, since we give a lot of EXP for this skill
        /// </summary>
        public override double ExperienceToLearningPointsConversionMultiplier => 0.50;

        public override string Name => "Hunting";

        public static bool ServerRollExtraLoot(DropItemContext context)
        {
            if (!context.HasCharacter)
            {
                return false;
            }

            var character = context.Character;
            var extraLootProbability = character.SharedGetFinalStatMultiplier(StatName.HuntingExtraLoot) - 1.0;
            return RandomHelper.RollWithProbability(extraLootProbability);
        }

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            byte maxLevel = 20;

            config.Category = GetCategory<SkillCategoryAgriculture>();
            config.MaxLevel = maxLevel;

            // extra loot chance bonus
            config.AddStatEffect(
                StatName.HuntingExtraLoot,
                level: 1,
                percentBonus: 2);

            config.AddStatEffect(
                StatName.HuntingExtraLoot,
                formulaPercentBonus: level => level);

            config.AddStatEffect(
                StatName.HuntingExtraLoot,
                level: maxLevel,
                percentBonus: 3);

            // corpse gathering speed bonus
            config.AddStatEffect(
                StatName.HuntingLootingSpeed,
                level: 1,
                percentBonus: 10);

            config.AddStatEffect(
                StatName.HuntingLootingSpeed,
                formulaPercentBonus: level => level * 4);

            config.AddStatEffect(
                StatName.HuntingLootingSpeed,
                level: maxLevel,
                percentBonus: 10);
        }
    }
}