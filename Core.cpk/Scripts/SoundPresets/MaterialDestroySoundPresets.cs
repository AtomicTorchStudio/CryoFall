namespace AtomicTorch.CBND.CoreMod.SoundPresets
{
    public static class MaterialDestroySoundPresets
    {
        public static readonly ReadOnlySoundPreset<ObjectMaterial> Default
            = new SoundPreset<ObjectMaterial>(
                  customDistance: (min: SoundConstants.AudioListenerMinDistanceObjectDestroy,
                                   max: SoundConstants.AudioListenerMaxDistanceObjectDestroy))
              .Add(ObjectMaterial.SoftTissues, "Destroy/SoftTissues")
              .Add(ObjectMaterial.HardTissues, "Destroy/HardTissues")
              .Add(ObjectMaterial.SolidGround, "Destroy/SolidGround")
              .Add(ObjectMaterial.Vegetation,  "Destroy/Vegetation")
              .Add(ObjectMaterial.Wood,        "Destroy/Wood")
              .Add(ObjectMaterial.Stone,       "Destroy/Stone")
              .Add(ObjectMaterial.Metal,       "Destroy/Metal")
              .Add(ObjectMaterial.Glass,       "Destroy/Glass");

        public static readonly ReadOnlySoundResourceSet TreeDestroy
            = new SoundResourceSet()
              .Add("Objects/Vegetation/ObjectTree/Destroy")
              .ToReadOnly();
    }
}