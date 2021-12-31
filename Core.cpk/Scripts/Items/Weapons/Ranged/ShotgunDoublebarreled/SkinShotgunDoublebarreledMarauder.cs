using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinShotgunDoublebarreledMarauder : ItemShotgunDoublebarreled
    {
        public override string Name => SkinName.Marauder;

        protected override Vector2D? MuzzleFlashTextureOffset => (309, 57);
    }
}