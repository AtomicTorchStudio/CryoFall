namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinRevolver8mmNoble : ItemRevolver8mm
    {
        public override string Name => SkinName.Noble;

        protected override Vector2D? MuzzleFlashTextureOffset => (166, 74);
    }
}