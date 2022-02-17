namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinRevolver8mmSheriff : ItemRevolver8mm
    {
        public override string Name => SkinName.Sheriff;

        protected override Vector2D? MuzzleFlashTextureOffset => (162, 68);
    }
}