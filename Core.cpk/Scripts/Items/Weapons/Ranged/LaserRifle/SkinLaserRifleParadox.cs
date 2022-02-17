namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinLaserRifleParadox : ItemLaserRifle
    {
        public override string Name => SkinName.Paradox;

        protected override Vector2D? MuzzleFlashTextureOffset => (235, 58);
    }
}