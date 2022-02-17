namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinFlintlockPistolEmperor : ItemFlintlockPistol
    {
        public override string Name => SkinName.Emperor;

        protected override Vector2D? MuzzleFlashTextureOffset => (186, 48);
    }
}