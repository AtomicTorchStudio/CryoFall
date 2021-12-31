using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinShotgunMilitaryPhoenix : ItemShotgunMilitary
    {
        public override string Name => SkinName.Phoenix;

        protected override Vector2D? MuzzleFlashTextureOffset => (248, 63);
    }
}