namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.CBND.GameApi.Resources;

    public class SkinLaserRiflePrototype : ItemLaserRifle
    {
        public override bool HasSkinCustomEffects => true;

        public override string Name => SkinName.Prototype;

        protected override TextureResource TextureResourceBeam { get; }
            = new("FX/WeaponTraces/BeamLaserBlue.png");

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.LaserBeamBlue;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.EnergyLaserWeaponBlue)
                       .Set(textureScreenOffset: (246, 53));
        }
    }
}