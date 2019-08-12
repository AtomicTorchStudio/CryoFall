namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementHaveSkills : QuestRequirementWithList<IProtoSkill>
    {
        public const string DescriptionFormat = "Learn skills: {0} to level {1}";

        private RequirementHaveSkills(byte minLevel, IReadOnlyList<IProtoSkill> list, ushort count, string description)
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

        public static RequirementHaveSkills RequireAny(
            ushort count,
            byte minLevel,
            string description = null)
        {
            var skills = Api.FindProtoEntities<IProtoSkill>();
            return RequireAny(skills, minLevel, count, description);
        }

        public static RequirementHaveSkills RequireAny(
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

            return new RequirementHaveSkills(minLevel, skills, count, description);
        }

        protected override bool ServerIsSatisfied(ICharacter character, QuestRequirementStateWithCount state)
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

            return base.ServerIsSatisfied(character, state);
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