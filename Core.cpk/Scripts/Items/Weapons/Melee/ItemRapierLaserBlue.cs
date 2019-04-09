namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Windows.Media;

    public class ItemRapierLaserBlue : ProtoItemRapierLaser
    {
        public override string Name => "Laser rapier (blue)";

        protected override Color LightColor => Color.FromArgb(0xDD, 0x66, 0x99, 0XFF);
    }
}