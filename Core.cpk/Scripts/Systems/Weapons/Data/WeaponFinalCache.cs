namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using JetBrains.Annotations;

    public class WeaponFinalCache
    {
        public readonly IReadOnlyList<DamageProportion> DamageDistributions;

        public readonly double DamageValue;

        public readonly double FinalDamageMultiplier;

        public readonly double InvertedArmorPiercingCoef;

        public readonly IProtoObjectExplosive ProtoObjectExplosive;

        public readonly IProtoItemWeapon ProtoWeapon;

        public readonly double RangeMax;

        public readonly IItem Weapon;

        public WeaponFinalCache(
            ICharacter character,
            FinalStatsCache characterFinalStatsCache,
            [CanBeNull] IItem weapon,
            [CanBeNull] IProtoItemWeapon protoWeapon,
            [CanBeNull] IProtoItemAmmo protoAmmo,
            DamageDescription damageDescription,
            IProtoObjectExplosive protoObjectExplosive = null)
        {
            this.Character = character;
            this.Weapon = weapon;
            this.ProtoWeapon = (IProtoItemWeapon)weapon?.ProtoItem ?? protoWeapon;
            this.ProtoAmmo = protoAmmo;
            this.ProtoObjectExplosive = protoObjectExplosive;

            if (damageDescription == null)
            {
                // TODO: it looks like not implemented yet and we should throw an exception here
                // fallback in case weapon don't provide damage description (such as no-ammo weapon)
                damageDescription = new DamageDescription(
                    damageValue: 0,
                    armorPiercingCoef: 0,
                    finalDamageMultiplier: 1,
                    rangeMax: 0,
                    damageDistribution: new DamageDistribution());
            }

            var descriptionDamages = damageDescription.DamageProportions;
            var damageDistributionsCount = descriptionDamages.Count;
            var resultDamageDistributions = new List<DamageProportion>(damageDistributionsCount);

            var totalPercents = 0d;

            for (var index = 0; index < damageDistributionsCount; index++)
            {
                var source = descriptionDamages[index];
                var statName = this.GetProportionStatName(source.DamageType);
                var resultDamageProportion = source.Proportion + characterFinalStatsCache[statName];
                if (resultDamageProportion <= 0)
                {
                    continue;
                }

                resultDamageDistributions.Add(new DamageProportion(source.DamageType, resultDamageProportion));
                totalPercents += resultDamageProportion;
            }

            if (damageDistributionsCount > 0
                && Math.Abs(totalPercents - 1) > 0.001d)
            {
                throw new Exception(
                    "Sum of all damage proportions must be exactly 1. Calculated value: "
                    + totalPercents.ToString("F3"));
            }

            this.DamageDistributions = resultDamageDistributions;

            this.DamageValue = damageDescription.DamageValue * (protoWeapon?.DamageMultiplier ?? 1.0)
                               + characterFinalStatsCache[StatName.DamageAdd];

            var weaponSkillProto = protoWeapon?.WeaponSkillProto;
            if (weaponSkillProto != null)
            {
                var statName = protoWeapon.WeaponSkillProto.StatNameDamageBonusMultiplier;
                this.DamageValue *= characterFinalStatsCache.GetMultiplier(statName);
            }

            this.RangeMax = damageDescription.RangeMax * (protoWeapon?.RangeMultipier ?? 1.0)
                            + characterFinalStatsCache[StatName.AttackRangeMax];

            var armorPiercingCoef = (1 + characterFinalStatsCache[StatName.AttackArmorPiercingMultiplier])
                                    * (damageDescription.ArmorPiercingCoef
                                       * (protoWeapon?.ArmorPiercingMultiplier ?? 1.0)
                                       + characterFinalStatsCache[StatName.AttackArmorPiercingValue]);

            this.InvertedArmorPiercingCoef = 1 - armorPiercingCoef;

            this.FinalDamageMultiplier = damageDescription.FinalDamageMultiplier
                                         + characterFinalStatsCache[StatName.AttackFinalDamageMultiplier];

            var probability = protoWeapon?.SpecialEffectProbability ?? 0;
            if (weaponSkillProto != null)
            {
                var statNameSpecialEffectChance = weaponSkillProto.StatNameSpecialEffectChanceMultiplier;
                probability *= characterFinalStatsCache.GetMultiplier(statNameSpecialEffectChance);
                this.SpecialEffectProbability = probability;
            }

            this.FireScatterPreset = protoAmmo?.OverrideFireScatterPreset 
                                   ?? protoWeapon?.FireScatterPreset
                                   ?? default;
            var shotsPerFire = this.FireScatterPreset.ProjectileAngleOffets.Length;
            if (shotsPerFire > 1)
            {
                // decrease final damage and special effect multiplier on the number of shots per fire
                var coef = 1.0 / shotsPerFire;
                this.FinalDamageMultiplier *= coef;
                this.SpecialEffectProbability *= coef;
            }
        }

        public ICharacter Character { get; }

        public WeaponFireScatterPreset FireScatterPreset { get; }

        public double SpecialEffectProbability { get; }

        public IProtoItemAmmo ProtoAmmo { get; }

        private StatName GetProportionStatName(DamageType damageType)
        {
            switch (damageType)
            {
                case DamageType.Impact:
                    return StatName.DamageProportionImpact;

                case DamageType.Kinetic:
                    return StatName.DamageProportionKinetic;

                case DamageType.Heat:
                    return StatName.DamageProportionHeat;

                case DamageType.Cold:
                    return StatName.DamageProportionCold;

                case DamageType.Chemical:
                    return StatName.DamageProportionChemical;

                case DamageType.Electrical:
                    return StatName.DamageProportionElectrical;

                case DamageType.Radiation:
                    return StatName.DamageProportionRadiation;

                case DamageType.Psi:
                    return StatName.DamageProportionPsi;

                default:
                    throw new ArgumentOutOfRangeException(nameof(damageType), damageType, null);
            }
        }
    }
}