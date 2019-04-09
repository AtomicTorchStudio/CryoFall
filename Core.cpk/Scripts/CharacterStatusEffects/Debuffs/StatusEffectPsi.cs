namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs.Client;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral;
    using AtomicTorch.CBND.CoreMod.Objects;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class StatusEffectPsi : ProtoRadiantStatusEffect
    {
        public override string Description =>
            "You are under the influence of a strong psi field, causing you to go insane. Leave the affected area or use psi-blocking medicine to prevent further damage.";

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Psi influence";

        protected override StatName DefenseStatName => StatName.DefensePsi;

        /// <summary>
        /// Time to remove full effect intensity back to zero in case the environmental intensity is 0.
        /// </summary>
        protected override double TimeToCoolDownToZeroSeconds => 3;

        /// <summary>
        /// Time to reach the full intensity in case the environmental intensity is 1.
        /// </summary>
        protected override double TimeToReachFullIntensitySeconds => 5;

        protected override void ClientDeinitialize(StatusEffectData data)
        {
            ClientComponentStatusEffectPsiManager.TargetIntensity = 0;
        }

        protected override void ClientUpdate(StatusEffectData data)
        {
            ClientComponentStatusEffectPsiManager.TargetIntensity = data.Intensity;
        }

        protected override void PrepareEffects(Effects effects)
        {
            // no health regeneration while under the effect of psi
            effects.AddPercent(this, StatName.HealthRegenerationPerSecond, -100);

            // reduced energy regeneration
            effects.AddPercent(this, StatName.StaminaRegenerationPerSecond, -50);
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            base.ServerUpdate(data);

            var damage = 5 * Math.Pow(data.Intensity, 1.5) * data.DeltaTime;

            if (data.Character.SharedHasStatusEffect<StatusEffectProtectionPsi>())
            {
                // has an active protection against psi damage
                damage *= 0.333;
            }

            if (data.Character.SharedHasStatusEffect<StatusEffectDrunk>())
            {
                // surprisingly alcohol providing some benefits :-)
                damage *= 0.667;
            }

            // reduce character health
            var stats = data.CharacterCurrentStats;
            var newHealth = stats.HealthCurrent - damage;

            stats.ServerSetHealthCurrent((float)newHealth);
        }

        protected override double SharedCalculateObjectEnvironmentalIntensity(
            ICharacter character,
            IWorldObject worldObject)
        {
            if (!(worldObject.ProtoWorldObject is IProtoObjectPsiSource protoPsiSource))
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

            var maxDistance = protoPsiSource.PsiRadiusMax;
            var minDistance = protoPsiSource.PsiRadiusMin;
            var distanceCoef = (distance - minDistance) / (maxDistance - minDistance);
            var intensity = 1 - MathHelper.Clamp(distanceCoef, 0, 1);

            return intensity * MathHelper.Clamp(protoPsiSource.PsiIntensity, 0, 1);
        }
    }
}