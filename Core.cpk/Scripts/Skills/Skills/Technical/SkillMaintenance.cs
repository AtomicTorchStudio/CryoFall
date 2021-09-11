namespace AtomicTorch.CBND.CoreMod.Skills
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class SkillMaintenance : ProtoSkill<SkillMaintenance.Flags>
    {
        // Base percent of durability restore when repairing an item (even when skill is 0 level).
        public const double BaseTinkerTableBonus = 40; // +40%

        public const string BaseTinkerTableBonus_TextFormat = "Tinker table base effectiveness {0}%";

        /// <summary>
        /// XP given for each individual item disassembled.
        /// </summary>
        public const double ExperiencePerItemDisassembled = 100.0;

        /// <summary>
        /// XP given for each individual item repaired.
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

        public override string Name => "Maintenance";

        public static byte SharedGetCurrentBonusPercent(ICharacter character)
        {
            var bonus = character.SharedGetFinalStatMultiplier(StatName.TinkerTableBonus) - 1.0;
            bonus = Math.Round(100 * bonus, MidpointRounding.AwayFromZero);
            return (byte)MathHelper.Clamp(bonus, 0, 100);
        }

        protected override void PrepareExtraDescriptionEntries(List<string> extraDescriptionEntries)
        {
            extraDescriptionEntries.Add(
                string.Format(BaseTinkerTableBonus_TextFormat,
                              (byte)Math.Round(BaseTinkerTableBonus, MidpointRounding.AwayFromZero)));
        }

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            config.Category = GetCategory<SkillCategoryTechnical>();

            // each level +2% bonus
            config.AddStatEffect(
                StatName.TinkerTableBonus,
                formulaPercentBonus: level => level * 2);

            config.AddFlagEffect(
                Flags.ChanceToRepairCompletely,
                level: 10);
        }
    }
}