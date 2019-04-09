namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    using static ObjectSoundMaterial;

    public static class MaterialHitsSoundPresets
    {
        private static readonly (float min, float max) HitSoundDistancePreset
            = (min: SoundConstants.AudioListenerMinDistanceObjectHit,
               max: SoundConstants.AudioListenerMaxDistanceObjectHit);

        public static readonly ReadOnlySoundPreset<ObjectSoundMaterial> Melee
            = new SoundPreset<ObjectSoundMaterial>(HitSoundDistancePreset)
              .Add(SoftTissues, "Hit/Melee/SoftTissues")
              .Add(HardTissues, "Hit/Melee/HardTissues")
              .Add(SolidGround, "Hit/Melee/SolidGround")
              .Add(Vegetation,  "Hit/Melee/Vegetation")
              .Add(Wood,        "Hit/Melee/Wood")
              .Add(Stone,       "Hit/Melee/Stone")
              .Add(Metal,       "Hit/Melee/Metal")
              .Add(Glass,       "Hit/Melee/Glass");

        public static readonly ReadOnlySoundPreset<ObjectSoundMaterial> MeleeEnergy
            = new SoundPreset<ObjectSoundMaterial>(HitSoundDistancePreset)
              .Add(SoftTissues, "Hit/Melee/Energy")
              .Add(HardTissues, "Hit/Melee/Energy")
              .Add(SolidGround, "Hit/Melee/Energy")
              .Add(Vegetation,  "Hit/Melee/Energy")
              .Add(Wood,        "Hit/Melee/Energy")
              .Add(Stone,       "Hit/Melee/Energy")
              .Add(Metal,       "Hit/Melee/Energy")
              .Add(Glass,       "Hit/Melee/Energy");

        public static readonly ReadOnlySoundPreset<ObjectSoundMaterial> Ranged
            = new SoundPreset<ObjectSoundMaterial>(HitSoundDistancePreset)
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
        public static readonly ReadOnlySoundPreset<ObjectSoundMaterial> RangedEnergy
            = MeleeEnergy;
    }
}