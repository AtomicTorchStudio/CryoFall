namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    // TODO: inherit from simple weapon class, not the melee weapon class
    public abstract class ProtoItemMobWeaponRanged : ProtoItemWeaponMelee
    {
        public override ushort AmmoCapacity => 0;

        public override double AmmoReloadDuration => 0;

        public override bool CanDamageStructures => false;

        public override CollisionGroup CollisionGroup => CollisionGroups.HitboxRanged;

        public override double DamageApplyDelay => 0;

        public override string Description => null;

        public override uint DurabilityMax => 0;

        public override ITextureResource Icon => null;

        public override bool IsLoopedAttackAnimation => false;

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
            List<IClientComponent> skeletonComponents)
        {
            // do nothing
        }

        public override string GetCharacterAnimationNameFire(ICharacter character)
        {
            var pi = MathConstants.PI;
            var piQuarter = pi / 4;

            var angle = character.ProtoCharacter.SharedGetRotationAngleRad(character);

            if (angle > piQuarter
                && angle < pi - piQuarter)
            {
                return "AttackRangedVertical";
            }

            if (angle > pi + piQuarter
                && angle < 2 * pi - piQuarter)
            {
                return "AttackRangedVertical";
            }

            return "AttackRangedHorizontal";
        }

        public override bool SharedCanSelect(IItem item, ICharacter character, bool isAlreadySelected, bool isByPlayer)
        {
            return character.ProtoCharacter is IProtoCharacterMob;
        }

        protected override ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit()
        {
            return MaterialHitsSoundPresets.Ranged;
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.SpecialUseSkeletonSound;
        }
    }
}