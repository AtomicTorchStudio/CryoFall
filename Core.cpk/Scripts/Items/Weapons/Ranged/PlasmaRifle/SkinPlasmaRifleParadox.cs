namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinPlasmaRifleParadox : ItemPlasmaRifle
    {
        public override string Name => SkinName.Paradox;

        protected override Vector2D? MuzzleFlashTextureOffset => (202, 65);
    }
}