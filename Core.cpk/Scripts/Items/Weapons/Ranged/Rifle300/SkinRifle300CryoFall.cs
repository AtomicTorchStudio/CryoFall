namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinRifle300CryoFall : ItemRifle300
    {
        public override string Name => SkinName.CryoFall;

        protected override Vector2D? MuzzleFlashTextureOffset => (357, 78);
    }
}