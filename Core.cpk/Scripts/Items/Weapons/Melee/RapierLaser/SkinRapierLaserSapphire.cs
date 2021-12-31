namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Windows.Media;

    public class SkinRapierLaserSapphire : ItemRapierLaser
    {
        public override string Name => SkinName.Sapphire;

        protected override Color LightColor => Color.FromArgb(0xDD, 0x66, 0x99, 0XFF);
    }
}