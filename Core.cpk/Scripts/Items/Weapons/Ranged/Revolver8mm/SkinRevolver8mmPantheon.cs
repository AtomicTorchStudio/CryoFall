namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinRevolver8mmPantheon : ItemRevolver8mm
    {
        public override string Name => SkinName.Pantheon;

        protected override Vector2D? MuzzleFlashTextureOffset => (164, 65);
    }
}