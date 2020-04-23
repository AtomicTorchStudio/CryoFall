namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskHaveSkill : BasePlayerTaskWithDefaultState
    {
        public const string DescriptionFormat = "Learn skill: {0} to level {1}";

        private TaskHaveSkill(IProtoSkill skill, byte minLevel, string description)
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

        public static TaskHaveSkill Require<TSkill>(byte minLevel, string description = null)
            where TSkill : IProtoSkill, new()
        {
            var skill = Api.GetProtoEntity<TSkill>();
            return Require(skill, minLevel, description);
        }

        public static TaskHaveSkill Require(IProtoSkill skill, byte minLevel, string description = null)
        {
            if (minLevel < 1)
            {
                throw new ArgumentException("Min level is 1", nameof(minLevel));
            }

            if (minLevel > skill.MaxLevel)
            {
                throw new ArgumentException(
                    $"Level cannot be larger than the max level of the skill: max level of {skill} is {skill.MaxLevel}",
                    nameof(minLevel));
            }

            return new TaskHaveSkill(skill, minLevel, description);
        }

        public override ITextureResource ClientCreateIcon()
        {
            return this.Skill.Icon;
        }

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskState state)
        {
            return character.SharedHasSkill(this.Skill, this.MinLevel);
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                PlayerCharacterSkills.ServerCharacterSkillLevelChanged +=
                    this.ServerCharacterSkillLevelChangedHandler;
            }
            else
            {
                PlayerCharacterSkills.ServerCharacterSkillLevelChanged -=
                    this.ServerCharacterSkillLevelChangedHandler;
            }
        }

        private void ServerCharacterSkillLevelChangedHandler(
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