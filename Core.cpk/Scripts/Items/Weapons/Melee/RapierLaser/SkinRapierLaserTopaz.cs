namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Windows.Media;

    public class SkinRapierLaserTopaz : ItemRapierLaser
    {
        public override string Name => SkinName.Topaz;

        protected override Color LightColor => Color.FromArgb(0xBB, 0xFF, 0xFF, 0xBB);
    }
}