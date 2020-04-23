namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TaskHaveSkills : BasePlayerTaskWithListAndCount<IProtoSkill>
    {
        public const string DescriptionFormat = "Learn skills: {0} to level {1}";

        private TaskHaveSkills(byte minLevel, IReadOnlyList<IProtoSkill> list, ushort count, string description)
            : base(list, count, description)
        {
            this.MinLevel = minLevel;
        }

        public override bool IsReversible => true;

        public byte MinLevel { get; }

        protected override string AutoDescription
            => string.Format(DescriptionFormat,
                             this.ListNames,
                             this.MinLevel);

        public static TaskHaveSkills RequireAny(
            ushort count,
            byte minLevel,
            string description = null)
        {
            var skills = Api.FindProtoEntities<IProtoSkill>();
            return RequireAny(skills, minLevel, count, description);
        }

        public static TaskHaveSkills RequireAny(
            IReadOnlyList<IProtoSkill> skills,
            byte minLevel,
            ushort count,
            string description = null)
        {
            if (minLevel < 1)
            {
                throw new ArgumentException("Min level is 1", nameof(minLevel));
            }

            if (count < 1)
            {
                throw new ArgumentException("Count cannot be < 1", nameof(count));
            }

            if (count > skills.Count)
            {
                throw new ArgumentException(
                    $"Count cannot be larger than the total amount of the skill: {count} specified but there are only {skills.Count} skills",
                    nameof(count));
            }

            foreach (var skill in skills)
            {
                if (minLevel > skill.MaxLevel)
                {
                    throw new ArgumentException(
                        $"Level cannot be larger than the max level of the skill: max level of {skill} is {skill.MaxLevel}",
                        nameof(minLevel));
                }
            }

            return new TaskHaveSkills(minLevel, skills, count, description);
        }

        public override ITextureResource ClientCreateIcon()
        {
            return this.List.Count == 1
                       ? this.List[0].Icon
                       : null;
        }

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskStateWithCount state)
        {
            var characterSkills = character.SharedGetSkills();
            var unlockedCount = 0;
            foreach (var entry in this.List)
            {
                if (!characterSkills.SharedHasSkill(entry, this.MinLevel))
                {
                    continue;
                }

                unlockedCount++;
                if (unlockedCount > this.RequiredCount)
                {
                    break;
                }
            }

            state.SetCountCurrent(unlockedCount, this.RequiredCount);

            return base.ServerIsCompleted(character, state);
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
            if (!this.List.Contains(skill))
            {
                return;
            }

            var context = this.GetActiveContext(character, out _);
            context?.Refresh();
        }
    }
}