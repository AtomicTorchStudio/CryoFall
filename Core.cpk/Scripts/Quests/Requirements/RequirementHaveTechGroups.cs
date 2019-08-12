namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class RequirementHaveTechGroups : QuestRequirementWithList<TechGroup>
    {
        public const string DescriptionFormat = "Unlock {0} tech groups: {1}";

        private RequirementHaveTechGroups(IReadOnlyList<TechGroup> list, ushort count, string description)
            : base(list, count, description)
        {
        }

        public override bool IsReversible => true;

        protected override string AutoDescription
            => string.Format(DescriptionFormat,
                             this.RequiredCount,
                             this.ListNames);

        public static RequirementHaveTechGroups RequireAny(
            IReadOnlyList<TechGroup> techGroups,
            ushort count,
            string description = null)
        {
            if (count < 1)
            {
                throw new ArgumentException("Count cannot be < 1", nameof(count));
            }

            if (count > techGroups.Count)
            {
                throw new ArgumentException(
                    $"Count cannot be larger than the total amount of the tech groups: {count} specified but there are only {techGroups.Count} tech groups",
                    nameof(count));
            }

            return new RequirementHaveTechGroups(techGroups, count, description);
        }

        public static RequirementHaveTechGroups RequireAnyFromTier(
            TechTier tier,
            ushort count,
            string description = null)
        {
            var techGroups = Api.FindProtoEntities<TechGroup>()
                                .Where(g => g.Tier == tier)
                                .ToList();

            return RequireAny(techGroups, count, description);
        }

        protected override bool ServerIsSatisfied(ICharacter character, QuestRequirementStateWithCount state)
        {
            var characterTechnologies = character.SharedGetTechnologies();
            var unlockedCount = 0;
            foreach (var entry in this.List)
            {
                if (!characterTechnologies.SharedIsGroupUnlocked(entry))
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
                PlayerCharacterTechnologies.CharacterGroupAddedOrRemoved += this.CharacterGroupAddedOrRemovedHandler;
            }
            else
            {
                PlayerCharacterTechnologies.CharacterGroupAddedOrRemoved -= this.CharacterGroupAddedOrRemovedHandler;
            }
        }

        private void CharacterGroupAddedOrRemovedHandler(ICharacter character, TechGroup techGroup, bool isAdded)
        {
            if (!this.List.Contains(techGroup))
            {
                return;
            }

            var context = this.GetActiveContext(character, out _);
            context?.Refresh();
        }
    }
}