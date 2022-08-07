﻿namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkinPlasmaRifleLightning : ItemPlasmaRifle
    {
        public override string Name => SkinName.Lightning;

        protected override Vector2D? MuzzleFlashTextureOffset => (246, 66);
    }
}