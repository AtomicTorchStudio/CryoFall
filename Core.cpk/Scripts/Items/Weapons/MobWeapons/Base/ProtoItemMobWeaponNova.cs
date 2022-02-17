namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class ProtoItemMobWeaponNova : ProtoItemWeaponMelee
    {
        public override ushort AmmoCapacity => 0;

        public override double AmmoReloadDuration => 0;

        public override IProtoItem BaseProtoItem => null;

        public override bool CanDamageStructures => false;

        public override CollisionGroup CollisionGroup => CollisionGroups.HitboxRanged;

        public override double DamageApplyDelay => 0;

        public override string Description => null;

        public override uint DurabilityMax => 0;

        public override bool IsLoopedAttackAnimation => false;

        public sealed override bool IsSkinnable => false;

        public override string Name => this.ShortId;

        public override double ReadyDelayDuration => 0;

        public override (float min, float max) SoundPresetWeaponDistance
            => (SoundConstants.AudioListenerMinDistanceRangedShot,
                SoundConstants.AudioListenerMaxDistanceRangedShotMobs);

        public override double SpecialEffectProbability =>
            1; // Must always be 1 for all animal weapons. Individual effects will be rolled in the effect function.

        protected override ProtoSkillWeapons WeaponSkill => null;

        protected override TextureResource WeaponTextureResource => null;

        public override void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            ProtoCharacterSkeleton protoCharacterSkeleton,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents,
            bool isPreview = false)
        {
            // do nothing
        }

        public override string GetCharacterAnimationNameFire(ICharacter character)
        {
            return "AttackAOE";
        }

        public virtual void ServerOnObjectHitByNova(
            IWorldObject damagedObject,
            double damageApplied,
            WeaponFinalCache weaponFinalCache)
        {
        }

        public override bool SharedCanSelect(IItem item, ICharacter character, bool isAlreadySelected, bool isByPlayer)
        {
            return character.ProtoCharacter is IProtoCharacterMob;
        }

        public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
        {
            if (IsClient)
            {
                return true;
            }

            var damageRadius = this.OverrideDamageDescription.RangeMax;

            WeaponExplosionSystem.ServerProcessExplosionCircle(
                positionEpicenter: character.Position + (0, this.SharedGetFireYOffset(character)),
                physicsSpace: Server.World.GetPhysicsSpace(),
                damageDistanceMax: damageRadius,
                weaponFinalCache: weaponState.WeaponCache,
                damageOnlyDynamicObjects: true,
                isDamageThroughObstacles: false,
                callbackCalculateDamageCoefByDistanceForStaticObjects:
                this.ServerCalculateDamageCoefficientByDistanceToTarget,
                callbackCalculateDamageCoefByDistanceForDynamicObjects:
                this.ServerCalculateDamageCoefficientByDistanceToTarget,
                collisionGroups: new[]
                {
                    CollisionGroups.HitboxMelee,
                    CollisionGroups.HitboxRanged,
                    CollisionGroup.Default
                });

            return true;
        }

        // no fire scatter and so no hit scan shots
        protected sealed override WeaponFireScatterPreset PrepareFireScatterPreset()
        {
            return new(Array.Empty<double>());
        }

        protected override ITextureResource PrepareIcon()
        {
            return null;
        }

        protected override ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit()
        {
            return MaterialHitsSoundPresets.Ranged;
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.SpecialUseSkeletonSound;
        }

        protected virtual double ServerCalculateDamageCoefficientByDistanceToTarget(double distance)
        {
            // full damage for half the range, then it's decreasing
            var damageRadius = this.OverrideDamageDescription.RangeMax;
            var distanceThreshold = damageRadius / 2;

            if (distance <= distanceThreshold)
            {
                return 1;
            }

            distance -= distanceThreshold;
            distance = Math.Max(0, distance);

            var maxDistance = damageRadius;
            maxDistance -= distanceThreshold;
            maxDistance = Math.Max(0, maxDistance);

            return 1 - Math.Min(distance / maxDistance, 1);
        }

        protected virtual float SharedGetFireYOffset(ICharacter character)
        {
            return ((IProtoCharacterCore)character.ProtoCharacter)
                .CharacterWorldWeaponOffsetRanged;
        }
    }
}