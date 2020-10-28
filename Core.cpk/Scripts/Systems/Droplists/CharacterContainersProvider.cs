namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class CharacterContainersProvider : IItemsContainerProvider
    {
        public CharacterContainersProvider(ICharacter character, bool includeEquipmentContainer)
        {
            this.Character = character;
            this.IncludeEquipmentContainer = includeEquipmentContainer;
        }

        public ICharacter Character { get; }

        public IEnumerable<IItemsContainer> Containers
        {
            get
            {
                if (this.Character.IsNpc)
                {
                    yield break;
                }

                yield return this.Character.SharedGetPlayerContainerInventory();
                yield return this.Character.SharedGetPlayerContainerHotbar();

                if (this.IncludeEquipmentContainer)
                {
                    yield return this.Character.SharedGetPlayerContainerEquipment();
                }
            }
        }

        public IEnumerable<IItemsContainer> ContainersForAddingIntoExistingStacksOnly
        {
            get
            {
                if (this.Character.IsNpc)
                {
                    yield break;
                }

                yield return this.Character.SharedGetPlayerContainerHotbar();
            }
        }

        public int EmptySlotsCount
        {
            get
            {
                var sum = 0;
                foreach (var c in this.Containers)
                {
                    sum += c.EmptySlotsCount;
                }

                return sum;
            }
        }

        public bool IncludeEquipmentContainer { get; }
    }
}