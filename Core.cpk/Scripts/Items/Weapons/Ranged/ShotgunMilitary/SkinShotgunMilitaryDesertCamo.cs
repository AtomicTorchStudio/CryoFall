using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinShotgunMilitaryDesertCamo : ItemShotgunMilitary
    {
        public override string Name => SkinName.DesertCamo;

        protected override Vector2D? MuzzleFlashTextureOffset => (234, 62);
    }
}