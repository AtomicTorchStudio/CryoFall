namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    public static class MaterialDestroySoundPresets
    {
        public static readonly ReadOnlySoundPreset<ObjectSoundMaterial> Default
            = new SoundPreset<ObjectSoundMaterial>(
                  customDistance: (min: SoundConstants.AudioListenerMinDistanceObjectDestroy,
                                   max: SoundConstants.AudioListenerMaxDistanceObjectDestroy))
              .Add(ObjectSoundMaterial.SoftTissues, "Destroy/SoftTissues")
              .Add(ObjectSoundMaterial.HardTissues, "Destroy/HardTissues")
              .Add(ObjectSoundMaterial.SolidGround, "Destroy/SolidGround")
              .Add(ObjectSoundMaterial.Vegetation,  "Destroy/Vegetation")
              .Add(ObjectSoundMaterial.Wood,        "Destroy/Wood")
              .Add(ObjectSoundMaterial.Stone,       "Destroy/Stone")
              .Add(ObjectSoundMaterial.Metal,       "Destroy/Metal")
              .Add(ObjectSoundMaterial.Glass,       "Destroy/Glass");

        public static readonly ReadOnlySoundResourceSet TreeDestroy
            = new SoundResourceSet()
              .Add("Objects/Vegetation/ObjectTree/Destroy")
              .ToReadOnly();
    }
}