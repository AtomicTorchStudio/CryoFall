namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Lights;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterEnergySystem;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class ProtoItemWeaponMeleeEnergy
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemWeaponMelee
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemWeaponEnergy
        where TPrivateState : WeaponPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public sealed override ushort AmmoCapacity => 0;

        public sealed override double AmmoReloadDuration => 0;

        public abstract double EnergyUsePerHit { get; }

        public abstract double EnergyUsePerShot { get; }

        public IReadOnlyItemLightConfig ItemLightConfig { get; set; }

        public override void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            ProtoCharacterSkeleton protoCharacterSkeleton,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            base.ClientSetupSkeleton(item, character, protoCharacterSkeleton, skeletonRenderer, skeletonComponents);

            if (!this.ItemLightConfig.IsLightEnabled)
            {
                return;
            }

            var sceneObject = character.ClientSceneObject;
            var componentLightSource = this.ClientCreateLightSource(item, character, sceneObject);
            var componentLightInSkeleton = sceneObject.AddComponent<ClientComponentLightInSkeleton>();
            componentLightInSkeleton.Setup(skeletonRenderer,
                                           this.ItemLightConfig,
                                           componentLightSource,
                                           "Weapon");

            skeletonComponents.Add(componentLightSource);
            skeletonComponents.Add(componentLightInSkeleton);
        }

        public override void ServerOnDamageApplied(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            double damage)
        {
            base.ServerOnDamageApplied(weaponCache, damagedObject, damage);

            if (IsClient)
            {
                // on client we cannot consume energy
                return;
            }

            // consume energy on hit
            var byCharacter = weaponCache.Character;
            var requiredEnergyAmount = SkillWeaponsEnergy.SharedGetRequiredEnergyAmount(byCharacter,
                                                                                        this.EnergyUsePerHit);
            CharacterEnergySystem.ServerDeductEnergyCharge(byCharacter, requiredEnergyAmount);
        }

        public override bool SharedCanFire(ICharacter character, WeaponState weaponState)
        {
            var requiredEnergyAmount = SkillWeaponsEnergy.SharedGetRequiredEnergyAmount(
                character,
                this.EnergyUsePerShot + this.EnergyUsePerHit);

            if (CharacterEnergySystem.SharedHasEnergyCharge(character, requiredEnergyAmount))
            {
                return true;
            }

            if (IsClient && weaponState.SharedGetInputIsFiring())
            {
                CharacterEnergySystem.ClientShowNotificationNotEnoughEnergyCharge(this);
                weaponState.ProtoWeapon.ClientItemUseFinish(weaponState.ItemWeapon);
                ClientHotbarSelectedItemManager.SelectedSlotId = null;
            }

            return false;
        }

        public override bool SharedCanSelect(IItem item, ICharacter character, bool isAlreadySelected)
        {
            if (!base.SharedCanSelect(item, character, isAlreadySelected))
            {
                return false;
            }

            var requiredEnergyAmount = SkillWeaponsEnergy.SharedGetRequiredEnergyAmount(
                character,
                this.EnergyUsePerShot + this.EnergyUsePerHit);

            if (CharacterEnergySystem.SharedHasEnergyCharge(character, requiredEnergyAmount))
            {
                return true;
            }

            // cannot select
            if (IsClient)
            {
                CharacterEnergySystem.ClientShowNotificationNotEnoughEnergyCharge(this);
            }

            return false;
        }

        // Please note: the check SharedCanFire() has been already passed
        public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
        {
            if (IsClient)
            {
                // on client we cannot consume energy
                return true;
            }

            // consume energy on fire
            var requiredEnergyAmount = SkillWeaponsEnergy.SharedGetRequiredEnergyAmount(
                character,
                this.EnergyUsePerShot);
            return CharacterEnergySystem.ServerDeductEnergyCharge(character, requiredEnergyAmount);
        }

        protected virtual BaseClientComponentLightSource ClientCreateLightSource(
            IItem item,
            ICharacter character,
            IClientSceneObject sceneObject)
        {
            return ClientLighting.CreateLightSourceSpot(
                sceneObject,
                this.ItemLightConfig);
        }

        protected sealed override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            compatibleAmmoProtos = null;
            var lightConfig = new ItemLightConfig() { IsLightEnabled = false };
            this.PrepareProtoWeaponEnergy(ref overrideDamageDescription, lightConfig);
            this.ItemLightConfig = lightConfig.ToReadOnly();
        }

        protected abstract void PrepareProtoWeaponEnergy(
            ref DamageDescription overrideDamageDescription,
            ItemLightConfig lightConfig);
    }

    public abstract class ProtoItemWeaponMeleeEnergy
        : ProtoItemWeaponMeleeEnergy
            <WeaponPrivateState, EmptyPublicState, EmptyClientState>
    {
    }
}