namespace AtomicTorch.CBND.CoreMod.Systems.Droplists
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class CharacterAndGroundContainersProvider : IItemsContainerProvider
    {
        public CharacterAndGroundContainersProvider(ICharacter character, Vector2Ushort tilePosition)
        {
            this.Character = character;
            this.TilePosition = tilePosition;
        }

        public ICharacter Character { get; }

        public IEnumerable<IItemsContainer> Containers
        {
            get
            {
                if (!this.Character.IsNpc)
                {
                    yield return this.Character.SharedGetPlayerContainerInventory();
                    yield return this.Character.SharedGetPlayerContainerHotbar();
                }

                this.CreateGroundContainer();
                yield return this.GroundContainer;
            }
        }

        public IEnumerable<IItemsContainer> ContainersForAddingIntoExistingStacksOnly
        {
            get
            {
                if (!this.Character.IsNpc)
                {
                    yield return this.Character.SharedGetPlayerContainerHotbar();
                }
            }
        }

        // always report that there is at least a single empty slot
        public int EmptySlotsCount => 1;

        public IItemsContainer GroundContainer { get; private set; }

        public Vector2Ushort TilePosition { get; }

        private void CreateGroundContainer()
        {
            var tile = Api.Server.World.GetTile(this.TilePosition);
            this.GroundContainer = ObjectGroundItemsContainer
                .ServerTryGetOrCreateGroundContainerAtTileOrNeighbors(this.Character, tile);

            if (this.GroundContainer is null)
            {
                // unable to create a ground container here
                return;
            }

            // don't restrict the ground container space limit
            Api.Server.Items.SetSlotsCount(this.GroundContainer, byte.MaxValue);
        }
    }
}