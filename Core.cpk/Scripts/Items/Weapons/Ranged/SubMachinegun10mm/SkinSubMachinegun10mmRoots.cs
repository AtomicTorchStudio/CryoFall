using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinSubMachinegun10mmRoots : ItemSubMachinegun10mm
    {
        public override string Name => SkinName.Roots;

        protected override Vector2D? MuzzleFlashTextureOffset => (199, 107);
    }
}