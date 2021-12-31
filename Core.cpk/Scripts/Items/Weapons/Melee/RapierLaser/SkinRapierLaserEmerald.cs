namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Windows.Media;

    public class SkinRapierLaserEmerald : ItemRapierLaser
    {
        public override string Name => SkinName.Emerald;

        protected override Color LightColor => Color.FromArgb(0xBB, 0xBB, 0xFF, 0xBB);
    }
}