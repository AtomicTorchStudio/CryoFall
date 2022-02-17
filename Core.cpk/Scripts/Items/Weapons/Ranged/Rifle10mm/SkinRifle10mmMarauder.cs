namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinRifle10mmMarauder : ItemRifle10mm
    {
        public override string Name => SkinName.Marauder;

        protected override Vector2D? MuzzleFlashTextureOffset => (318, 58);
    }
}