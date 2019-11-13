namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    using static ObjectMaterial;

    public static class MaterialHitsSoundPresets
    {
        private static readonly (float min, float max) HitSoundDistancePreset
            = (min: SoundConstants.AudioListenerMinDistanceObjectHit,
               max: SoundConstants.AudioListenerMaxDistanceObjectHit);

        public static readonly ReadOnlySoundPreset<ObjectMaterial> Melee
            = new SoundPreset<ObjectMaterial>(HitSoundDistancePreset)
              .Add(SoftTissues, "Hit/Melee/SoftTissues")
              .Add(HardTissues, "Hit/Melee/HardTissues")
              .Add(SolidGround, "Hit/Melee/SolidGround")
              .Add(Vegetation,  "Hit/Melee/Vegetation")
              .Add(Wood,        "Hit/Melee/Wood")
              .Add(Stone,       "Hit/Melee/Stone")
              .Add(Metal,       "Hit/Melee/Metal")
              .Add(Glass,       "Hit/Melee/Glass");

        public static readonly ReadOnlySoundPreset<ObjectMaterial> MeleeEnergy
            = new SoundPreset<ObjectMaterial>(HitSoundDistancePreset)
              .Add(SoftTissues, "Hit/Melee/Energy")
              .Add(HardTissues, "Hit/Melee/Energy")
              .Add(SolidGround, "Hit/Melee/Energy")
              .Add(Vegetation,  "Hit/Melee/Energy")
              .Add(Wood,        "Hit/Melee/Energy")
              .Add(Stone,       "Hit/Melee/Energy")
              .Add(Metal,       "Hit/Melee/Energy")
              .Add(Glass,       "Hit/Melee/Energy");

        // used for fists and some animals
        public static readonly ReadOnlySoundPreset<ObjectMaterial> MeleeNoWeapon
            = new SoundPreset<ObjectMaterial>(HitSoundDistancePreset)
              .Add(SoftTissues, "Hit/Melee/SoftTissues")
              .Add(HardTissues, "Hit/Melee/HardTissues")
              .Add(SolidGround, "Hit/Melee/SolidGround")
              .Add(Vegetation,  "Hit/Melee/Vegetation")
              .Add(Wood,        "Hit/Melee/Wood")
              .Add(Stone,       "Hit/Melee/HardTissues")
              .Add(Metal,       "Hit/Melee/HardTissues")
              .Add(Glass,       "Hit/Melee/HardTissues");

        public static readonly ReadOnlySoundPreset<ObjectMaterial> Ranged
            = new SoundPreset<ObjectMaterial>(HitSoundDistancePreset)
              .Add(SoftTissues, "Hit/Ranged/SoftTissues")
              .Add(HardTissues, "Hit/Ranged/HardTissues")
              .Add(SolidGround, "Hit/Ranged/SolidGround")
              .Add(Vegetation,  "Hit/Ranged/Vegetation")
              .Add(Wood,        "Hit/Ranged/Wood")
              .Add(Stone,       "Hit/Ranged/Stone")
              .Add(Metal,       "Hit/Ranged/Metal")
              .Add(Glass,       "Hit/Ranged/Glass");

        /// <summary>
        /// TODO: currently there are no separate sounds for ranged energy weapons, but later we need to add them
        /// </summary>
        public static readonly ReadOnlySoundPreset<ObjectMaterial> RangedEnergy
            = MeleeEnergy;
    }
}