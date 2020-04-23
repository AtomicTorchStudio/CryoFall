namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class TaskHaveTechGroups : BasePlayerTaskWithListAndCount<TechGroup>
    {
        public const string DescriptionFormat = "Unlock {0} tech groups: {1}";

        private TaskHaveTechGroups(IReadOnlyList<TechGroup> list, ushort count, string description)
            : base(list, count, description)
        {
        }

        public override bool IsReversible => true;

        protected override string AutoDescription
            => string.Format(DescriptionFormat,
                             this.RequiredCount,
                             this.ListNames);

        public static TaskHaveTechGroups RequireAny(
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

            return new TaskHaveTechGroups(techGroups, count, description);
        }

        public static TaskHaveTechGroups RequireAnyFromTier(
            TechTier tier,
            ushort count,
            string description = null)
        {
            var techGroups = TechGroup.AvailableTechGroups
                                      .Where(g => g.Tier == tier)
                                      .ToList();

            return RequireAny(techGroups, count, description);
        }

        protected override bool ServerIsCompleted(ICharacter character, PlayerTaskStateWithCount state)
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

            return base.ServerIsCompleted(character, state);
        }

        protected override void SetTriggerActive(bool isActive)
        {
            if (isActive)
            {
                PlayerCharacterTechnologies.ServerCharacterGroupAddedOrRemoved +=
                    this.ServerCharacterGroupAddedOrRemovedHandler;
            }
            else
            {
                PlayerCharacterTechnologies.ServerCharacterGroupAddedOrRemoved -=
                    this.ServerCharacterGroupAddedOrRemovedHandler;
            }
        }

        private void ServerCharacterGroupAddedOrRemovedHandler(ICharacter character, TechGroup techGroup, bool isAdded)
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