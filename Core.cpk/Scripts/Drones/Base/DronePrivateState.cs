namespace AtomicTorch.CBND.CoreMod.Drones
{
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class DronePrivateState : BasePrivateState
    {
        public IItem AssociatedItem { get; set; }

        public IItem AssociatedItemReservedSlot { get; set; }

        public ICharacter CharacterOwner { get; set; }

        public bool IsDespawned { get; set; } = true;

        [TempOnly]
        public FinalStatsCache LastCharacterOwnerFinalStatsCache { get; set; }

        public IItemsContainer ReservedItemsContainer { get; set; }

        public IItemsContainer StorageItemsContainer { get; set; }

        [TempOnly]
        public double WeaponCooldownSecondsRemains { get; set; }

        [TempOnly]
        public WeaponFinalCache WeaponFinalCache { get; set; }
    }
}