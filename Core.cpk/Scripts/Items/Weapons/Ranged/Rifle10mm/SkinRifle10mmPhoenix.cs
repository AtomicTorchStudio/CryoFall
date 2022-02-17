namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinRifle10mmPhoenix : ItemRifle10mm
    {
        public override string Name => SkinName.Phoenix;

        protected override Vector2D? MuzzleFlashTextureOffset => (323, 64);
    }
}