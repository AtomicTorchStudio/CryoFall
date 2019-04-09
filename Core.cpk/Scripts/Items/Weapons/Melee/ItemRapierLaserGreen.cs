namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Windows.Media;

    public class ItemRapierLaserGreen : ProtoItemRapierLaser
    {
        public override string Name => "Laser rapier (green)";

        protected override Color LightColor => Color.FromArgb(0xBB, 0xBB, 0xFF, 0xBB);
    }
}