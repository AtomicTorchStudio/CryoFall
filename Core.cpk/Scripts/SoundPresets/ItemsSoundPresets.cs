namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    using AtomicTorch.CBND.GameApi.Resources;
    using static ItemSound;

    public static class ItemsSoundPresets
    {
        public static readonly ReadOnlySoundPreset<ItemSound> ItemGeneric
            = new SoundPreset<ItemSound>().Add(Pick, "Items/Pick")
                                          .Add(Drop,     "Items/Drop")
                                          .Add(Use,      "Items/Use")
                                          .Add(Equip,    "Items/Equip")
                                          .Add(Unequip,  "Items/Unequip")
                                          .Add(Broken,   "Items/Broken")
                                          .Add(Select,   "Items/Select")
                                          .Add(Deselect, "Items/Deselect");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemDataLog
            = ItemGeneric.Clone()
                         .Replace(Use, "Items/DataLogs/Use");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemFertilizer
            = ItemGeneric.Clone()
                         .Replace(Use, "Items/Fertilizer/Use");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemFood
            = ItemGeneric.Clone()
                         .Replace(Use, "Items/Food/Use");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemFoodCrunchy
            = ItemFood.Clone()
                      .Replace(Use, "Items/Food/UseCrunchy");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemFoodDrink
            = ItemFood.Clone()
                      .Replace(Use, "Items/Food/UseDrink");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemFoodDrinkAlcohol
            = ItemFood.Clone()
                      .Replace(Use, "Items/Food/UseDrinkAlcohol");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemFoodDrinkCan
            = ItemFood.Clone()
                      .Replace(Use, "Items/Food/UseDrinkCan");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemFoodFruit
            = ItemFood.Clone()
                      .Replace(Use, "Items/Food/UseFruit");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemMedical
            = ItemGeneric.Clone()
                         .Replace(Use, "Items/Medical/Use");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemMedicalBandage
            = ItemMedical.Clone()
                         .Replace(Use, "Items/Medical/UseBandage");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemMedicalMedkit
            = ItemMedical.Clone()
                         .Replace(Use, "Items/Medical/UseMedkit");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemMedicalSmoke
            = ItemMedical.Clone()
                         .Replace(Use, "Items/Medical/UseSmoke");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemMedicalStimpack
            = ItemMedical.Clone()
                         .Replace(Use, "Items/Medical/UseStimpack");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemMedicalTablets
            = ItemMedical.Clone()
                         .Replace(Use, "Items/Medical/UseTablets");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemSeed
            = ItemGeneric.Clone()
                         .Replace(Use, "Items/Seeds/Use");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemWeaponMelee
            = ItemGeneric.Clone()
                         .Replace(Broken, "Weapons/Melee/Broken");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemWeaponMeleeEnergyLaserRapier
            = ItemWeaponMelee.Clone()
                             .Replace(Broken,   "Weapons/MeleeEnergy/LaserRapier/Broken")
                             .Replace(Select,   "Weapons/MeleeEnergy/LaserRapier/Select")
                             .Replace(Deselect, "Weapons/MeleeEnergy/LaserRapier/Deselect")
                             .Replace(Idle,     "Weapons/MeleeEnergy/LaserRapier/Idle");

        public static readonly ReadOnlySoundPreset<ItemSound> ItemWeaponRanged
            = ItemGeneric.Clone()
                         .Replace(Broken, "Weapons/Ranged/Broken");

        public static readonly SoundResource SoundResourceOtherPlayerDropItem
            = new SoundResource("Items/OtherPlayerDrop");

        public static readonly SoundResource SoundResourceOtherPlayerPickItem
            = new SoundResource("Items/OtherPlayerPick");
    }
}