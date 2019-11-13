namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class ProtoItemWeaponRanged
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemWeapon
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemWeaponRanged
        where TPrivateState : WeaponPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public override bool CanBeSelectedInVehicle => true;

        public override string CharacterAnimationAimingName => "WeaponRifleAiming";

        public virtual double CharacterAnimationAimingRecoilDuration => 0.3;

        public virtual string CharacterAnimationAimingRecoilName => "WeaponRifleShooting";

        public virtual double CharacterAnimationAimingRecoilPower => 1;

        public virtual double CharacterAnimationAimingRecoilPowerAddCoef => 1;

        /// <summary>
        /// Ranged weapon don't have fire animation. It uses recoil animation instead.
        /// <see cref="CharacterAnimationAimingRecoilDuration" />.
        /// </summary>
        public override double FireAnimationDuration => 0;

        public override double FireInterval => 0.5;

        public override double FirePatternCooldownDuration => this.FireInterval + 0.4;

        public override ITextureResource Icon => new TextureResource("Items/Weapons/Ranged/" + this.GetType().Name);

        public IMuzzleFlashDescriptionReadOnly MuzzleFlashDescription { get; private set; }

        public override (float min, float max) SoundPresetWeaponDistance
            => (SoundConstants.AudioListenerMinDistanceRangedShot,
                SoundConstants.AudioListenerMaxDistanceRangedShot);

        public override void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            ProtoCharacterSkeleton protoCharacterSkeleton,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            // preload the muzzle flash sprite sheet
            Client.Rendering.PreloadTextureAsync(this.MuzzleFlashDescription.TextureAtlas);

            base.ClientSetupSkeleton(item, character, protoCharacterSkeleton, skeletonRenderer, skeletonComponents);
        }

        protected abstract void PrepareMuzzleFlashDescription(MuzzleFlashDescription description);

        protected sealed override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            if (IsClient)
            {
                var description = new MuzzleFlashDescription();
                description.Set(MuzzleFlashPresets.Default);
                this.PrepareMuzzleFlashDescription(description);
                this.MuzzleFlashDescription = description;
            }

            this.PrepareProtoWeaponRanged(out compatibleAmmoProtos, ref overrideDamageDescription);
        }

        protected abstract void PrepareProtoWeaponRanged(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription);

        protected override ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit()
        {
            return MaterialHitsSoundPresets.Ranged;
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemWeaponRanged;
        }

        protected override ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon()
        {
            return WeaponsSoundPresets.WeaponRanged;
        }

        protected override void ServerOnDegradeWeapon(
            ICharacter character,
            IItem weaponItem,
            IProtoItemWeapon protoWeapon,
            IReadOnlyList<IWorldObject> hitObjects)
        {
            // decrease durability on every shot
            ItemDurabilitySystem.ServerModifyDurability(
                weaponItem,
                delta: -this.DurabilityDecreasePerAction);
        }
    }

    public abstract class ProtoItemWeaponRanged
        : ProtoItemWeaponRanged
            <WeaponPrivateState, EmptyPublicState, EmptyClientState>
    {
    }
}