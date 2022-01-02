﻿using AtomicTorch.GameEngine.Common.Primitives;

namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged
{
    public class SkinRifle10mmPhoenix : ItemRifle10mm
    {
        public override string Name => SkinName.Phoenix;

        protected override Vector2D? MuzzleFlashTextureOffset => (323, 64);
    }
}