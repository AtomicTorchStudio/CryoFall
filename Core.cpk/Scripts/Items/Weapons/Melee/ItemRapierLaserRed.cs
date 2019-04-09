namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Windows.Media;

    public class ItemRapierLaserRed : ProtoItemRapierLaser
    {
        public override string Name => "Laser rapier (red)";

        protected override Color LightColor => Color.FromArgb(0xCC, 0xFF, 0xAA, 0xAA);
    }
}