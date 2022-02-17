namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinLaserPistolRedLine : ItemLaserPistol
    {
        public override bool HasSkinCustomEffects => true;

        public override string Name => SkinName.RedLine;

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.LaserRed;
        }

        protected override void PrepareMuzzleFlashDescription(MuzzleFlashDescription description)
        {
            description.Set(MuzzleFlashPresets.EnergyLaserWeaponRed)
                       .Set(textureScreenOffset: (153, 81));
        }
    }
}