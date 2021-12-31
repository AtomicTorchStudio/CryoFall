using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinCrossbowPrimitive : ItemCrossbow
    {
        public override string Name => SkinName.Primitive;

        protected override Vector2D? MuzzleFlashTextureOffset => (223, 64);
        
    }
}