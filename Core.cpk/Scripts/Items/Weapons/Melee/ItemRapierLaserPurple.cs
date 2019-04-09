namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Windows.Media;

    public class ItemRapierLaserPurple : ProtoItemRapierLaser
    {
        public override string Name => "Laser rapier (purple)";

        protected override Color LightColor => Color.FromArgb(0xCC, 0xBB, 0x88, 0XFF);
    }
}