namespace AtomicTorch.CBND.CoreMod.Skills
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public class SkillLumbering : ProtoSkill<SkillLumbering.Flags>
    {
        public const double ExperienceAddPerStructurePoint = 0.25; // (600 HP -> 150 XP)

        public static readonly DropItemConditionDelegate ConditionGetExtraSapplings
            // requires selected woodcutting tool and flag
            = context => context.HasCharacter
                         && context.ByWeaponProto is IProtoItemToolWoodcutting
                         && context.Character.SharedHasSkillFlag(Flags.GetExtraSapplings);

        public static readonly DropItemConditionDelegate ConditionGetSapplings
            // requires selected woodcutting tool and flag
            = context => context.HasCharacter
                         && context.ByWeaponProto is IProtoItemToolWoodcutting
                         && context.Character.SharedHasSkillFlag(Flags.GetSapplings);

        public enum Flags
        {
            [Description("Chance to get saplings")]
            GetSapplings,

            [Description("Extra saplings chance")]
            GetExtraSapplings
        }

        public override string Description =>
            "Better training and understanding of this profession improve your woodcutting speed.";

        public override double ExperienceToLearningPointsConversionMultiplier =>
            0.4; // lower since each tree gives 150 exp (which would mean 1.5LP without this multiplier)

        public override string Name => "Lumbering";

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            config.Category = GetCategory<SkillCategoryProduction>();

            // level bonus
            config.AddStatEffect(
                StatName.WoodcuttingSpeed,
                formulaPercentBonus: level => level * 2);

            // lvl 5 bonus
            config.AddFlagEffect(
                Flags.GetSapplings,
                level: 5);

            // lvl 5 bonus
            config.AddFlagEffect(
                Flags.GetExtraSapplings,
                level: 15);

            // lvl 20 bonus
            config.AddStatEffect(
                StatName.WoodcuttingSpeed,
                level: 20,
                percentBonus: 10);
        }
    }
}