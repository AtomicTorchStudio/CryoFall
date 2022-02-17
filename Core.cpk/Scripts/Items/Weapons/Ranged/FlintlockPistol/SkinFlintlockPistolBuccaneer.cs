namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinFlintlockPistolBuccaneer : ItemFlintlockPistol
    {
        public override string Name => SkinName.Buccaneer;

        protected override Vector2D? MuzzleFlashTextureOffset => (180, 54);
    }
}