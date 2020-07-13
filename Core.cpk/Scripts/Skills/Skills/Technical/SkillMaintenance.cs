namespace AtomicTorch.CBND.CoreMod.Skills
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class SkillMaintenance : ProtoSkill<SkillMaintenance.Flags>
    {
        // Base percent of durability restore when repairing an item (even when skill is 0 level).
        public const double BaseTinkerTableBonus = 20; // +20%

        /// <summary>
        /// Exp given for each individual item repaired.
        /// </summary>
        public const double ExperiencePerItemRepaired = 1000.0;

        public enum Flags
        {
            [Description("Chance to repair item completely")]
            ChanceToRepairCompletely
        }

        public override string Description =>
            "Allows you to repair and maintain different items and equipment. Applies to work performed at tinker table.";

        public override double ExperienceToLearningPointsConversionMultiplier =>
            0.1f; // essentially 1 LP per repair, higher values would prompt players to grind-repair for LP

        public override bool IsSharingLearningPointsWithPartyMembers => true;

        public override string Name => "Maintenance";

        public static byte SharedGetCurrentBonusPercent(ICharacter character)
        {
            var bonus = character.SharedGetFinalStatMultiplier(StatName.TinkerTableBonus) - 1.0;
            bonus = Math.Round(100 * bonus, MidpointRounding.AwayFromZero);
            return (byte)MathHelper.Clamp(bonus, 0, 100);
        }

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            config.Category = GetCategory<SkillCategoryTechnical>();

            // each level +1% bonus
            config.AddStatEffect(
                StatName.TinkerTableBonus,
                formulaPercentBonus: level => level);

            config.AddFlagEffect(
                Flags.ChanceToRepairCompletely,
                level: 10);
        }
    }
}