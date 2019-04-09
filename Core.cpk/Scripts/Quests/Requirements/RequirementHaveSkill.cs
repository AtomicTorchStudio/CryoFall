namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementHaveSkill : QuestRequirementWithDefaultState
    {
        public const string DescriptionFormat = "Learn skill: {0} to level {1}";

        private RequirementHaveSkill(IProtoSkill skill, byte minLevel, string description)
            : base(description)
        {
            this.Skill = skill;
            this.MinLevel = minLevel;
        }

        public override bool IsReversible => true;

        public byte MinLevel { get; }

        public IProtoSkill Skill { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat,
                             this.Skill.Name,
                             this.MinLevel);

        public static RequirementHaveSkill Require<TSkill>(byte minLevel, string description = null)
            where TSkill : IProtoSkill, new()
        {
            if (minLevel < 1)
            {
                throw new ArgumentException("Min level is 1", nameof(minLevel));
            }

            var skill = Api.GetProtoEntity<TSkill>();
            if (minLevel > skill.MaxLevel)
            {
                throw new ArgumentException(
                    $"Level cannot be larger than the max level of the skill: max level of {skill} is {skill.MaxLevel}",
                    nameof(minLevel));
            }

            return new RequirementHaveSkill(skill, minLevel, description);
        }

        protected override bool ServerIsSatisfied(ICharacter character, QuestRequirementState state)
        {
            return character.SharedHasSkill(this.Skill, this.MinLevel);
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                PlayerCharacterSkills.CharacterSkillLevelChanged +=
                    this.CharacterSkillLevelChangedHandler;
            }
            else
            {
                PlayerCharacterSkills.CharacterSkillLevelChanged -=
                    this.CharacterSkillLevelChangedHandler;
            }
        }

        private void CharacterSkillLevelChangedHandler(
            ICharacter character,
            IProtoSkill skill,
            SkillLevelData skillLevelData)
        {
            if (skill != this.Skill)
            {
                return;
            }

            var context = this.GetActiveContext(character, out _);
            context?.Refresh();
        }
    }
}