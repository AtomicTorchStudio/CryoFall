namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Windows.Media;

    public class SkinRapierLaserTourmaline : ItemRapierLaser
    {
        public override string Name => SkinName.Tourmaline;

        protected override Color LightColor => Color.FromArgb(0xCC, 0xBB, 0x88, 0XFF);
    }
}