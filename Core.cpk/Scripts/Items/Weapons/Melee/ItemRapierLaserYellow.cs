namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Windows.Media;

    public class ItemRapierLaserYellow : ProtoItemRapierLaser
    {
        public override string Name => "Laser rapier (yellow)";

        protected override Color LightColor => Color.FromArgb(0xBB, 0xFF, 0xFF, 0xBB);
    }
}