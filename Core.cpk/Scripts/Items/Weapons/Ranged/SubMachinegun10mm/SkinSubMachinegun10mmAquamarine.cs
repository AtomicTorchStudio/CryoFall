using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinSubMachinegun10mmAquamarine : ItemSubMachinegun10mm
    {
        public override string Name => SkinName.Aquamarine;

        protected override Vector2D? MuzzleFlashTextureOffset => (217, 98);
    }
}