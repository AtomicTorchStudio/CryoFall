namespace AtomicTorch.CBND.CoreMod.Items.Weapons.MobWeapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemWeaponGenericAnimalStrong : ProtoItemWeaponMelee
    {
        public override bool CanDamageStructures => false;

        public override string Description => null;

        public override uint DurabilityMax => 0;

        public override double FireInterval => 1.5;

        public override ITextureResource Icon => null;

        public override string Name => this.ShortId;

        public override double SpecialEffectProbability =>
            1; // Must always be 1 for all animal weapons. Individual effects will be rolled in the effect function.

        protected override ProtoSkillWeapons WeaponSkill => GetSkill<SkillWeaponsMelee>();

        protected override TextureResource WeaponTextureResource => null;

        public override void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            // do nothing
        }

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            // no ammo used
            compatibleAmmoProtos = null;

            overrideDamageDescription = new DamageDescription(
                damageValue: 40,
                armorPiercingCoef: 0.1,
                finalDamageMultiplier: 1,
                rangeMax: 1.5,
                damageDistribution: new DamageDistribution(DamageType.Impact, 1));
        }

        protected override ReadOnlySoundPreset<ObjectSoundMaterial> PrepareSoundPresetHit()
        {
            return MaterialHitsSoundPresets.MeleeSoftTissuesOnly;
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.SpecialUseSkeletonSound;
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            if (RandomHelper.RollWithProbability(0.25))
            {
                damagedCharacter.ServerAddStatusEffect<StatusEffectBleeding>(intensity: 0.2);
                damagedCharacter.ServerAddStatusEffect<StatusEffectPain>(intensity: 0.2);
            }
        }
    }
}