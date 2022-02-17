namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinSteppenHawkNecromancer : ItemSteppenHawk
    {
        public override string Name => SkinName.Necromancer;

        protected override Vector2D? MuzzleFlashTextureOffset => (160, 82);
    }
}