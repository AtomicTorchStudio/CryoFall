using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinRifle300Aquamarine : ItemRifle300
    {
        public override string Name => SkinName.Aquamarine;

        protected override Vector2D? MuzzleFlashTextureOffset => (341, 77);
    }
}