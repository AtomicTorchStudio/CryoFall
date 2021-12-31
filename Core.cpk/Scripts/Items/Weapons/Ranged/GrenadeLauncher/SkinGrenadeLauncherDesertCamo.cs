using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinGrenadeLauncherDesertCamo : ItemGrenadeLauncher
    {
        public override string Name => SkinName.DesertCamo;

        protected override Vector2D? MuzzleFlashTextureOffset => (276, 52);
    }
}