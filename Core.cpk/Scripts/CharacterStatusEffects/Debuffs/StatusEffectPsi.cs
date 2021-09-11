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

    public class StatusEffectPsi : ProtoRadiantStatusEffect
    {
        public const double DamagePerSecondByIntensity = 5;

        public override string Description =>
            "You are under the influence of a strong psi field, causing you to go insane. Leave the affected area or use psi-blocking medicine to prevent further damage.";

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Psi influence";

        public override double ServerUpdateIntervalSeconds => 0.5;

        protected override StatName DefenseStatName => StatName.DefensePsi;

        /// <summary>
        /// Time to remove full effect intensity back to zero when the environmental intensity is 0.
        /// </summary>
        protected override double TimeToCoolDownToZeroSeconds => 3;

        /// <summary>
        /// Time to reach the full intensity when the environmental intensity is 1.
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

            // add info to tooltip that this effect deals damage
            effects.AddValue(this, StatName.VanityContinuousDamage, 1);
        }

        protected override double ServerCalculateObjectEnvironmentalIntensity(
            ICharacter character,
            IWorldObject worldObject)
        {
            if (worldObject.ProtoWorldObject is not IProtoObjectPsiSource protoPsiSource
                || !protoPsiSource.ServerIsPsiSourceActive(worldObject))
            {
                return 0;
            }

            if (protoPsiSource is IProtoObjectPsiSourceCustom protoPsiSourceCustom)
            {
                return MathHelper.Clamp(
                    protoPsiSourceCustom.ServerCalculatePsiIntensity(worldObject, character),
                    min: 0,
                    max: 1);
            }

            var position = worldObject switch
            {
                IStaticWorldObject staticWorldObject
                    => staticWorldObject.TilePosition.ToVector2D()
                       + staticWorldObject.ProtoStaticWorldObject.Layout.Center,

                IDynamicWorldObject dynamicWorldObject
                    => dynamicWorldObject.Position,

                _ => throw new InvalidOperationException()
            };

            var distance = position.DistanceTo(character.Position);

            var maxDistance = protoPsiSource.PsiRadiusMax;
            var minDistance = protoPsiSource.PsiRadiusMin;
            var distanceCoef = (distance - minDistance) / (maxDistance - minDistance);
            var intensity = 1 - MathHelper.Clamp(distanceCoef, 0, 1);

            return intensity * MathHelper.Clamp(protoPsiSource.PsiIntensity, 0, 1);
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            base.ServerUpdate(data);

            if (!data.Character.ServerIsOnline)
            {
                return;
            }

            // calculate based damage for a given delta time
            var damage = DamagePerSecondByIntensity * Math.Pow(data.Intensity, 1.5) * data.DeltaTime;

            // modify damage based on effect multiplier
            damage *= data.Character.SharedGetFinalStatMultiplier(StatName.PsiEffectMultiplier);

            data.CharacterCurrentStats.ServerReduceHealth(damage, data.StatusEffect);
        }
    }
}