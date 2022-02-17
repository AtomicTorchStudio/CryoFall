namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinShotgunDoublebarreledGraphene : ItemShotgunDoublebarreled
    {
        public override string Name => SkinName.Graphene;

        protected override Vector2D? MuzzleFlashTextureOffset => (301, 55);
    }
}