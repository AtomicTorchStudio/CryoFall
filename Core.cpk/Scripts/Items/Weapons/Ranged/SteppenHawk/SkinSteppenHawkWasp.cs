namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinSteppenHawkWasp : ItemSteppenHawk
    {
        public override string Name => SkinName.Wasp;

        protected override Vector2D? MuzzleFlashTextureOffset => (169, 72);
    }
}