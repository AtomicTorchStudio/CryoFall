namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinRifle300Aquamarine : ItemRifle300
    {
        public override string Name => SkinName.Aquamarine;

        protected override Vector2D? MuzzleFlashTextureOffset => (341, 77);
    }
}