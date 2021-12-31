namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Windows.Media;

    public class ItemRapierLaser : ProtoItemRapierLaser
    {
        public override string Name => "Laser rapier";

        public override double SkeletonPreviewOffsetX => 120;

        protected override Color LightColor => Color.FromArgb(0xAA, 0xFF, 0xFF, 0XFF);
    }
}