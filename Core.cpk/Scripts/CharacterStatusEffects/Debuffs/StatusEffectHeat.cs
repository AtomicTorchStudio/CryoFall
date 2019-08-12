namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs.Client;
    using AtomicTorch.CBND.CoreMod.Objects;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class StatusEffectHeat : ProtoRadiantStatusEffect
    {
        public const double DamagePerSecondByIntensity = 10;

        public override string Description =>
            "You are exposed to a high level of heat from a nearby heat source. Immediately leave the area to prevent further damage.";

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Heat";

        protected override StatName DefenseStatName => StatName.DefenseHeat;

        /// <summary>
        /// Time to remove full effect intensity back to zero in case the environmental intensity is 0.
        /// </summary>
        protected override double TimeToCoolDownToZeroSeconds => 3;

        /// <summary>
        /// Time to reach the full intensity in case the environmental intensity is 1.
        /// </summary>
        protected override double TimeToReachFullIntensitySeconds => 4;

        protected override void ClientDeinitialize(StatusEffectData data)
        {
            ClientComponentStatusEffectHeatManager.TargetIntensity = 0;
        }

        protected override void ClientUpdate(StatusEffectData data)
        {
            ClientComponentStatusEffectHeatManager.TargetIntensity = data.Intensity;
        }

        protected override void PrepareEffects(Effects effects)
        {
            // add info to tooltip that this effect deals damage
            effects.AddValue(this, StatName.VanityContinuousDamage, 1);
        }

        protected override void ServerAddIntensity(StatusEffectData data, double intensityToAdd)
        {
            intensityToAdd *= data.Character.SharedGetFinalStatMultiplier(StatName.HeatIncreaseRateMultiplier);

            base.ServerAddIntensity(data, intensityToAdd);
        }

        protected override double ServerCalculateObjectEnvironmentalIntensity(
            ICharacter character,
            IWorldObject worldObject)
        {
            if (!(worldObject.ProtoWorldObject is IProtoObjectHeatSource protoHeatSource))
            {
                return 0;
            }

            Vector2D position;
            switch (worldObject)
            {
                case IStaticWorldObject staticWorldObject:
                    position = staticWorldObject.TilePosition.ToVector2D()
                               + staticWorldObject.ProtoStaticWorldObject.Layout.Center;
                    break;
                case IDynamicWorldObject dynamicWorldObject:
                    position = dynamicWorldObject.Position;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var distance = position.DistanceTo(character.Position);

            var maxDistance = protoHeatSource.HeatRadiusMax;
            var minDistance = protoHeatSource.HeatRadiusMin;
            var distanceCoef = (distance - minDistance) / (maxDistance - minDistance);
            var intensity = 1 - MathHelper.Clamp(distanceCoef, 0, 1);

            return intensity * MathHelper.Clamp(protoHeatSource.HeatIntensity, 0, 1);
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            base.ServerUpdate(data);

            var damage = DamagePerSecondByIntensity
                         * Math.Pow(data.Intensity, 1.5)
                         * data.DeltaTime;

            // modify damage based on effect multiplier
            damage *= data.Character.SharedGetFinalStatMultiplier(StatName.HeatEffectMultiplier);

            // modify damage based on armor
            // divided by 2 because otherwise many armor pieces would give practically complete immunity to heat
            // so 100% armor would give 50% reduction in damage
            var defenseHeat = data.Character.SharedGetFinalStatValue(StatName.DefenseHeat);
            damage *= Math.Max(0, 1 - defenseHeat / 2.0);

            data.CharacterCurrentStats.ServerReduceHealth(damage, this);
        }
    }
}