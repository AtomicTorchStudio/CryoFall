using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinShotgunDoublebarreledEmperor : ItemShotgunDoublebarreled
    {
        public override string Name => SkinName.Emperor;

        protected override Vector2D? MuzzleFlashTextureOffset => (305, 56);
    }
}