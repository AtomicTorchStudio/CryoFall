namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    using static ObjectSound;

    public static class ObjectsSoundsPresets
    {
        public static readonly ReadOnlySoundPreset<ObjectSound> ObjectGeneric
            = new SoundPreset<ObjectSound>()
              .Add(Active,             "Objects/Active")
              .Add(Place,              "Objects/Structures/Place")
              .Add(InteractSuccess,    "Objects/InteractSuccess")
              .Add(InteractFail,       "Objects/InteractFail")
              .Add(InteractOutOfRange, "Objects/InteractOutOfRange");

        public static readonly ReadOnlySoundPreset<ObjectSound> ObjectConstructionSite
            = ObjectGeneric.Clone()
                           // the interact sound is defined by the toolbox item
                           //.Replace(InteractProcess, "Objects/Structures/ConstructionSite/InteractProcess")
                           .Replace(InteractSuccess, "Objects/Structures/ConstructionSite/InteractSuccess");

        // corpse looting sound
        public static readonly ReadOnlySoundPreset<ObjectSound> ObjectCorpse
            = ObjectGeneric.Clone()
                           .Replace(InteractStart,   "Objects/Corpse/InteractStart")
                           .Replace(InteractProcess, "Objects/Corpse/InteractProcess")
                           .Replace(InteractSuccess, "Objects/Corpse/InteractSuccess")
                           .Replace(InteractFail,    "Objects/Corpse/InteractFail");

        // pry open sounds
        public static readonly ReadOnlySoundPreset<ObjectSound> ObjectLockedContainer
            = ObjectGeneric.Clone()
                           .Replace(InteractStart,   "Objects/LockedContainer/InteractStart")
                           .Replace(InteractProcess, "Objects/LockedContainer/InteractProcess")
                           .Replace(InteractSuccess, "Objects/LockedContainer/InteractSuccess")
                           .Replace(InteractFail,    "Objects/LockedContainer/InteractFail");

        // vegetation gathering sounds
        public static readonly ReadOnlySoundPreset<ObjectSound> ObjectVegetation
            = ObjectGeneric.Clone()
                           .Replace(InteractStart,   "Objects/Vegetation/InteractStart")
                           .Replace(InteractProcess, "Objects/Vegetation/InteractProcess")
                           .Replace(InteractSuccess, "Objects/Vegetation/InteractSuccess")
                           .Replace(InteractFail,    "Objects/Vegetation/InteractFail");

        // TODO: we need proper sounds for searching garbage piles
        public static readonly ReadOnlySoundPreset<ObjectSound> ObjectGarbagePile
            = ObjectLockedContainer;

        public static readonly ReadOnlySoundPreset<ObjectSound> ObjectHackableContainer
            = ObjectGeneric.Clone()
                           .Clear(InteractStart)
                           .Clear(InteractFail)
                           .Replace(InteractProcess, "Events/HackingProcess")
                           .Replace(InteractSuccess, "Events/HackingFinish");
    }
}