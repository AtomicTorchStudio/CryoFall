namespace AtomicTorch.CBND.CoreMod.Skills
{
    using static Stats.StatName;

    public class SkillCyberneticAffinity : ProtoSkill
    {
        /// <summary>
        /// Experience added when installing an implant.
        /// </summary>
        public const double ExperienceAddedPerImplantInstalled = 500.0;

        /// <summary>
        /// Experience added per second while wearing any implant
        /// (if two implants are equipped then this value is taken for each, basically 2x).
        /// </summary>
        public const double ExperienceAddedPerImplantPerSecond = 1.0;

        /// <summary>
        /// Experience added when removing any implant (or broken implant).
        /// </summary>
        public const double ExperienceAddedPerImplantUninstalled = 100.0;

        public override string Description =>
            "Continuously exposing your body to cybernetic technology made you and your immune system more accustomed to their presence.";

        /// <summary>
        /// This is 1/5 of the standard conversion, because this skill takes no effort from the player to advance
        /// </summary>
        public override double ExperienceToLearningPointsConversionMultiplier => 0.2;

        public override bool IsSharingLearningPointsWithPartyMembers => false;

        public override string Name => "Cybernetic affinity";

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            byte maxLevel = 20;

            config.Category = GetCategory<SkillCategoryPersonal>();
            config.MaxLevel = maxLevel;

            // 2 effects at max level:
            // -40% durability loss from damage taken (but not death)
            // -40% durability loss from time
            var stats = new[]
            {
                ImplantDegradationFromDamageMultiplier,
                ImplantDegradationSpeedMultiplier,
            };

            foreach (var statName in stats)
            {
                config.AddStatEffect(
                    statName,
                    formulaPercentBonus: level => -level);

                config.AddStatEffect(
                    statName,
                    level: 10,
                    percentBonus: -5);

                config.AddStatEffect(
                    statName,
                    level: 15,
                    percentBonus: -5);

                config.AddStatEffect(
                    statName,
                    level: maxLevel,
                    percentBonus: -10);
            }
        }
    }
}