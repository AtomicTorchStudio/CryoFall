namespace AtomicTorch.CBND.CoreMod.Items.Weapons.Melee
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ItemNoWeapon : ProtoItemWeaponMelee
    {
        public static ItemNoWeapon Instance { get; private set; }

        public override double DamageApplyDelay => 0.075;

        [NotLocalizable]
        public override string Description => "Fallback weapon prototype in case no weapon selected.";

        public override uint DurabilityMax => 0;

        public override double FireAnimationDuration => 0.6;

        public override string Name => "No weapon";

        // TODO: the ready cooldown indicator is not displayed over the no-weapon slot
        // but it still could inherit delay from previous weapon
        public override double ReadyDelayDuration => 0;

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

        protected override WeaponFireTracePreset PrepareFireTracePreset()
        {
            return WeaponFireTracePresets.NoWeapon;
        }

        protected override ITextureResource PrepareIcon()
        {
            return null;
        }

        protected override void PrepareProtoItem()
        {
            base.PrepareProtoItem();
            Instance = this;
        }

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            // no ammo used
            compatibleAmmoProtos = null;

            overrideDamageDescription = new DamageDescription(
                damageValue: 10,
                armorPiercingCoef: 0,
                finalDamageMultiplier: 1,
                rangeMax: 1.2,
                damageDistribution: new DamageDistribution(DamageType.Impact, proportion: 1));
        }

        protected override ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit()
        {
            return MaterialHitsSoundPresets.MeleeNoWeapon;
        }
    }
}