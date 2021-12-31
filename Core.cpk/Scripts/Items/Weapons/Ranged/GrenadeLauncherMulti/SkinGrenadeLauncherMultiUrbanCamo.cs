using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinGrenadeLauncherMultiUrbanCamo : ItemGrenadeLauncherMulti
    {
        public override string Name => SkinName.UrbanCamo;

        protected override Vector2D? MuzzleFlashTextureOffset => (244, 63);
    }
}