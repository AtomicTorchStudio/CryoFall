namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinLaserRifleZenith : ItemLaserRifle
    {
        public override string Name => SkinName.Zenith;

        protected override Vector2D? MuzzleFlashTextureOffset => (243, 47);
    }
}