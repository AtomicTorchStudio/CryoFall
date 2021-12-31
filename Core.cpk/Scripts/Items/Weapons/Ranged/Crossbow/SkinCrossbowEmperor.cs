using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinCrossbowEmperor : ItemCrossbow
    {
        public override string Name => SkinName.Emperor;

        protected override Vector2D? MuzzleFlashTextureOffset => (247, 73);
    }
}