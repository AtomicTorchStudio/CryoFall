namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinShotgunDoublebarreledMarauder : ItemShotgunDoublebarreled
    {
        public override string Name => SkinName.Marauder;

        protected override Vector2D? MuzzleFlashTextureOffset => (309, 57);
    }
}