using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinShotgunMilitaryUrbanCamo : ItemShotgunMilitary
    {
        public override string Name => SkinName.UrbanCamo;

        protected override Vector2D? MuzzleFlashTextureOffset => (251, 63);
    }
}