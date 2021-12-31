namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinMusketRedwood : ItemMusket
    {
        public override string Name => SkinName.Redwood;

        protected override Vector2D? MuzzleFlashTextureOffset => (332, 51);
    }
}