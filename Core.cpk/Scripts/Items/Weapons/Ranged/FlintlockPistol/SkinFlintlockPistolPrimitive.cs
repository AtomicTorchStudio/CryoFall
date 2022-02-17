namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinFlintlockPistolPrimitive : ItemFlintlockPistol
    {
        public override string Name => SkinName.Primitive;

        protected override Vector2D? MuzzleFlashTextureOffset => (167, 58);
    }
}