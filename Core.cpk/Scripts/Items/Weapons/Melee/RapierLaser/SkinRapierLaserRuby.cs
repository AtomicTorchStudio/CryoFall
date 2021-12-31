namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Windows.Media;

    public class SkinRapierLaserRuby : ItemRapierLaser
    {
        public override string Name => SkinName.Ruby;

        protected override Color LightColor => Color.FromArgb(0xCC, 0xFF, 0xAA, 0xAA);
    }
}