using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinRifleBoltActionNoble : ItemRifleBoltAction
    {
        public override string Name => SkinName.Noble;

        protected override Vector2D? MuzzleFlashTextureOffset => (333, 63);
    }
}