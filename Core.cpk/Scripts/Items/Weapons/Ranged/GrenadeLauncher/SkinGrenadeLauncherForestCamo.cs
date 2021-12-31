using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinGrenadeLauncherForestCamo : ItemGrenadeLauncher
    {
        public override string Name => SkinName.ForestCamo;

        protected override Vector2D? MuzzleFlashTextureOffset => (251, 96);
    }
}