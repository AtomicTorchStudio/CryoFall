namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public static class ClientGrassRenderingHelper
    {
        private static readonly EffectResource EffectResource = new EffectResource("Grass");

        private static readonly Dictionary<GrassAnimationPreset, RenderingMaterial> Materials
            = new Dictionary<GrassAnimationPreset, RenderingMaterial>();

        public static void Setup(
            IComponentSpriteRenderer renderer,
            float power,
            float pivotY,
            bool canFlip = true)
        {
            var worldObject = renderer.SceneObject.AttachedWorldObject;
            if (worldObject == null)
            {
                // blueprint - use default rendering
                return;
            }

            var isRenderingFlipped = canFlip
                                     && PositionalRandom.Get(worldObject.TilePosition, 0, 3, seed: 9125835) == 0;

            var phaseOffset = (byte)PositionalRandom.Get(worldObject.TilePosition, 0, 8, seed: 614392664);

            var preset = new GrassAnimationPreset(power, pivotY, phaseOffset, isRenderingFlipped);
            if (!Materials.TryGetValue(preset, out var material))
            {
                // no cached material found for the required preset - create and setup new material
                Materials[preset] = material = RenderingMaterial.Create(EffectResource);
                material.EffectParameters
                        .Set("Flip",        isRenderingFlipped ? 1 : 0)
                        .Set("Power",       power)
                        .Set("PivotY",      pivotY)
                        .Set("PhaseOffset", phaseOffset);
            }

            renderer.RenderingMaterial = material;
        }

        private readonly struct GrassAnimationPreset
            : IEquatable<GrassAnimationPreset>
        {
            public readonly bool IsFlip;

            public readonly byte PhaseOffset;

            public readonly float PivotY;

            public readonly float Power;

            public GrassAnimationPreset(float power, float pivotY, byte phaseOffset, bool isFlip)
            {
                this.IsFlip = isFlip;
                this.Power = power;
                this.PivotY = pivotY;
                this.PhaseOffset = phaseOffset;
            }

            public bool Equals(GrassAnimationPreset other)
            {
                return this.IsFlip == other.IsFlip
                       && this.PivotY.Equals(other.PivotY)
                       && this.Power.Equals(other.Power)
                       && this.PhaseOffset == other.PhaseOffset;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is GrassAnimationPreset other && this.Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = this.IsFlip.GetHashCode();
                    hashCode = (hashCode * 397) ^ this.PivotY.GetHashCode();
                    hashCode = (hashCode * 397) ^ this.Power.GetHashCode();
                    hashCode = (hashCode * 397) ^ this.PhaseOffset.GetHashCode();
                    return hashCode;
                }
            }
        }
    }
}