namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinPlasmaRifleRedMarble : ItemPlasmaRifle
    {
        public override string Name => SkinName.RedMarble;

        protected override Vector2D? MuzzleFlashTextureOffset => (205, 69);
    }
}