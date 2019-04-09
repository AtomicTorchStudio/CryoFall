namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Windows.Media;

    public class ItemRapierLaserWhite : ProtoItemRapierLaser
    {
        public override string Name => "Laser rapier (white)";

        protected override Color LightColor => Color.FromArgb(0xAA, 0xFF, 0xFF, 0XFF);
    }
}