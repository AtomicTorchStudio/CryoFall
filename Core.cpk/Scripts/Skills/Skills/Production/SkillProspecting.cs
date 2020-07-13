namespace AtomicTorch.CBND.CoreMod.Skills
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class SkillProspecting : ProtoSkill<SkillProspecting.Flags>
    {
        public const double ExperienceAddPerStructurePoint = 0.12; // (1000 HP -> 120 XP)

        public static readonly DropItemConditionDelegate ConditionAdditionalYield
            // requires selected mining tool/bomb and flag
            = context => context.HasCharacter
                         && (context.ByWeaponProto is IProtoItemToolMining
                             || context.ByExplosiveProto != null)
                         && context.Character.SharedHasSkillFlag(Flags.AdditionalYield);

        public static readonly DropItemConditionDelegate ConditionDropGemstones
            // requires selected mining tool/bomb and flag
            = context => context.HasCharacter
                         && (context.ByWeaponProto is IProtoItemToolMining
                             || context.ByExplosiveProto != null)
                         && context.Character.SharedHasSkillFlag(Flags.FindGemstones);

        public enum Flags
        {
            [Description("Chance to have additional yield")]
            AdditionalYield,

            [Description("Chance to find gemstones when mining")]
            FindGemstones
        }

        public override string Description =>
            "Better knowledge of mineral deposits increases your excavation speed, yield and chances to find rare resources.";

        public override double ExperienceToLearningPointsConversionMultiplier =>
            0.6; // lower since each mineral gives 120 exp (which would mean 1.2LP without this multiplier)

        public override bool IsSharingLearningPointsWithPartyMembers => true;

        public override string Name => "Prospecting";

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            config.Category = GetCategory<SkillCategoryProduction>();

            // every level bonus
            config.AddStatEffect(
                StatName.MiningSpeed,
                formulaPercentBonus: level => level * 2);

            // lvl 10 bonus
            config.AddFlagEffect(
                Flags.AdditionalYield,
                level: 10);

            // lvl 15 bonus
            config.AddFlagEffect(
                Flags.FindGemstones,
                level: 15);

            // lvl 20 bonus
            config.AddStatEffect(
                StatName.MiningSpeed,
                level: 20,
                percentBonus: 10);
        }
    }
}