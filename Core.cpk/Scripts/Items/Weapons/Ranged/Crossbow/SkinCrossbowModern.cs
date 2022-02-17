namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinCrossbowModern : ItemCrossbow
    {
        public override string Name => SkinName.Modern;

        protected override Vector2D? MuzzleFlashTextureOffset => (247, 64);
    }
}