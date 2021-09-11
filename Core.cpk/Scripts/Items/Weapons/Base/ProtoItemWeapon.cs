namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Damage;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    [PrepareOrder(afterType: typeof(IProtoItemAmmo))]
    public abstract class ProtoItemWeapon
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItem
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemWeapon
        where TPrivateState : WeaponPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        private readonly Lazy<ProtoSkillWeapons> lazyWeaponSkillPrototype;

        private ClientInputContext helperInputListener;

        protected ProtoItemWeapon()
        {
            this.lazyWeaponSkillPrototype = new Lazy<ProtoSkillWeapons>(
                () => this.WeaponSkill);
        }

        public abstract ushort AmmoCapacity { get; }

        public virtual ushort AmmoConsumptionPerShot => this.AmmoCapacity > 0
                                                            ? (ushort)1
                                                            : (ushort)0;

        public abstract double AmmoReloadDuration { get; }

        public virtual bool CanDamageStructures => true;

        public abstract string CharacterAnimationAimingName { get; }

        public abstract CollisionGroup CollisionGroup { get; }

        public IReadOnlyList<IProtoItemAmmo> CompatibleAmmoProtos { get; private set; }

        public abstract double DamageApplyDelay { get; }

        public virtual double DamageMultiplier => 1;

        public abstract DamageStatsComparisonPreset DamageStatsComparisonPreset { get; }

        public virtual ushort DurabilityDecreasePerAction => 1;

        public abstract uint DurabilityMax { get; }

        public abstract double FireAnimationDuration { get; }

        public abstract double FireInterval { get; }

        public virtual double FirePatternCooldownDuration => 0;

        public WeaponFirePatternPreset FirePatternPreset { get; private set; }

        public WeaponFireScatterPreset FireScatterPreset { get; private set; }

        public WeaponFireTracePreset FireTracePreset { get; private set; }

        public override double GroundIconScale => 1.5;

        public virtual bool IsLoopedAttackAnimation => (this.FireInterval / this.FireAnimationDuration) < 1.5;

        public virtual bool IsRepairable => true;

        public sealed override ushort MaxItemsPerStack => 1;

        public DamageDescription OverrideDamageDescription { get; private set; }

        public virtual double RangeMultiplier => 1;

        /// <inheritdoc />
        public abstract double ReadyDelayDuration { get; }

        public IProtoItemAmmo ReferenceAmmoProto
        {
            get
            {
                foreach (var protoAmmo in this.CompatibleAmmoProtos)
                {
                    if (protoAmmo.IsReferenceAmmo)
                    {
                        return protoAmmo;
                    }
                }

                return this.CompatibleAmmoProtos.FirstOrDefault();
            }
        }

        public virtual float ShotVolumeMultiplier => 1.0f;

        /// <inheritdoc />
        public ReadOnlySoundPreset<ObjectMaterial> SoundPresetHit { get; private set; }

        public ReadOnlySoundPreset<WeaponSound> SoundPresetWeapon { get; private set; }

        public virtual (float min, float max) SoundPresetWeaponDistance
            => (SoundConstants.AudioListenerMinDistance, SoundConstants.AudioListenerMaxDistance);

        public virtual (float min, float max) SoundPresetWeaponDistance3DSpread
            => default;

        public virtual double SpecialEffectProbability => 0;

        public virtual string WeaponAttachmentName => "WeaponRifle";

        public ProtoSkillWeapons WeaponSkillProto => this.lazyWeaponSkillPrototype.Value;

        ITextureResource IProtoItemWeapon.WeaponTextureResource => this.CachedWeaponTextureResource;

        protected TextureResource CachedWeaponTextureResource { get; private set; }

        protected abstract ProtoSkillWeapons WeaponSkill { get; }

        protected virtual TextureResource WeaponTextureResource
            => new("Characters/Weapons/" + this.GetType().Name,
                   isProvidesMagentaPixelPosition: true);

        public Control ClientCreateHotbarOverlayControl(IItem item)
        {
            return new HotbarItemWeaponOverlayControl(item);
        }

        public void ClientCreateItemSlotOverlayControls(IItem item, List<Control> controls)
        {
            if (this.DurabilityMax > 0)
            {
                controls.Add(ItemSlotDurabilityOverlayControl.Create(item));
            }
        }

        public virtual void ClientOnFireModChanged(bool isFiring, uint shotsDone)
        {
            var weaponState = ClientCurrentCharacterHelper.PrivateState.WeaponState;
            WeaponSystem.Instance.CallServer(
                _ => _.ServerRemote_SetWeaponFiringMode(isFiring, shotsDone, weaponState.ProtoWeapon));
        }

        public virtual void ClientOnWeaponHitOrTrace(
            ICharacter firingCharacter,
            Vector2D worldPositionSource,
            IProtoItemWeapon protoWeapon,
            IProtoItemAmmo protoAmmo,
            IProtoCharacter protoCharacter,
            in Vector2Ushort fallbackCharacterPosition,
            IReadOnlyList<WeaponHitData> hitObjects,
            in Vector2D endPosition,
            bool endsWithHit)
        {
        }

        public virtual void ClientOnWeaponShot(ICharacter character)
        {
        }

        public virtual void ClientPlayWeaponHitSound(
            [CanBeNull] IWorldObject hitWorldObject,
            IProtoWorldObject protoWorldObject,
            WeaponFireScatterPreset fireScatterPreset,
            ObjectMaterial objectMaterial,
            Vector2D worldObjectPosition)
        {
            // apply some volume variation
            var volume = SoundConstants.VolumeHit;
            volume *= RandomHelper.Range(0.8f, 1.0f);
            var pitch = RandomHelper.Range(0.95f, 1.05f);

            // adjust volume proportionally to the number of scattered projects per fire
            var projectilesCount = fireScatterPreset.ProjectileAngleOffets.Length;
            volume *= (float)Math.Pow(1.0 / projectilesCount, 0.35);

            var soundPresetHit = (protoWorldObject as IDamageableProtoWorldObject)?.OverrideSoundPresetHit
                                 ?? this.SoundPresetHit;

            if (hitWorldObject is not null)
            {
                soundPresetHit.PlaySound(
                    objectMaterial,
                    hitWorldObject,
                    volume: volume,
                    pitch: pitch);
            }
            else
            {
                soundPresetHit.PlaySound(
                    objectMaterial,
                    protoWorldObject,
                    worldPosition: worldObjectPosition,
                    volume: volume,
                    pitch: pitch);
            }
        }

        public virtual void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            ProtoCharacterSkeleton protoCharacterSkeleton,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            this.ClientPreloadTextures();

            protoCharacterSkeleton.ClientSetupItemInHand(
                skeletonRenderer,
                this.WeaponAttachmentName,
                this.CachedWeaponTextureResource);
        }

        public virtual string GetCharacterAnimationNameFire(ICharacter character)
        {
            // no animation by default
            return null;
        }

        public virtual void ServerOnItemBrokeAndDestroyed(IItem item, IItemsContainer container, byte slotId)
        {
            if (container is null
                || container.IsDestroyed)
            {
                return;
            }

            // try to unload the ammo
            var privateState = GetPrivateState(item);
            var ammoCount = privateState.AmmoCount;
            if (ammoCount == 0)
            {
                return;
            }

            privateState.SetAmmoCount(0);

            // try spawn into the destroyed item slot
            var result = Server.Items.CreateItem(
                container: container,
                protoItem: privateState.CurrentProtoItemAmmo,
                count: ammoCount,
                slotId: slotId);
            if (result.IsEverythingCreated
                || container.OwnerAsCharacter is null)
            {
                // spawned successfully or the owner is not a character
                return;
            }

            // spawned unsuccessfully - try to spawn in any container for character
            result.Rollback();
            Server.Items.CreateItem(
                protoItem: privateState.CurrentProtoItemAmmo,
                toCharacter: container.OwnerAsCharacter,
                count: ammoCount);
        }

        public virtual void ServerOnItemDamaged(IItem item, double damageApplied)
        {
            ItemDurabilitySystem.ServerModifyDurability(item, delta: -(int)damageApplied);
        }

        public void ServerOnShot(
            ICharacter character,
            IItem weaponItem,
            IProtoItemWeapon protoWeapon,
            IReadOnlyList<IWorldObject> hitObjects)
        {
            // try reduce weapon durability
            // for ranged weapon - when shot
            // for melee weapon - when at least one object was hit
            var characterSkills = character.SharedGetSkills();
            if (characterSkills is null)
            {
                // not a player character - don't degrade the weapon
                return;
            }

            if (weaponItem is not null)
            {
                ServerItemUseObserver.NotifyItemUsed(character, weaponItem);
            }

            var shouldDegrade = true;
            if (this.WeaponSkillProto is not null)
            {
                // get degradation probability
                var probability = character.SharedGetFinalStatMultiplier(
                    this.WeaponSkillProto.StatNameDegrationRateMultiplier);
                probability = MathHelper.Clamp(probability, 0, 1);
                if (!RandomHelper.RollWithProbability(probability))
                {
                    shouldDegrade = false;
                }
            }

            if (shouldDegrade)
            {
                this.ServerOnDegradeWeapon(character, weaponItem, protoWeapon, hitObjects);
            }
        }

        public virtual bool SharedCanFire(ICharacter character, WeaponState weaponState)
        {
            if (this.AmmoCapacity == 0)
            {
                // weapon doesn't use ammo
                return true;
            }

            // weapon uses ammo
            var itemWeapon = weaponState.ItemWeapon;
            if (itemWeapon is null)
            {
                return false;
            }

            if (itemWeapon.ProtoItem != this)
            {
                Logger.Error($"Weapon proto mismatch: CanFire check for weapon {itemWeapon} executed for proto {this}",
                             character);
                return false;
            }

            var weaponPrivateState = GetPrivateState(itemWeapon);
            if (weaponPrivateState.AmmoCount >= this.AmmoConsumptionPerShot)
            {
                return true;
            }

            // not enough ammo - try auto-reloading of the same ammo type
            var isInputFiring = weaponState.SharedGetInputIsFiring();
            if (!weaponState.IsIdleAutoReloadingAllowed
                && !isInputFiring)
            {
                // do not attempt idle auto-reloading (when client is not firing)
                return false;
            }

            if (!isInputFiring
                && weaponPrivateState.CurrentProtoItemAmmo is null)
            {
                // do not attempt to idle auto-reload the ammo as no ammo type is selected for this weapon
                return false;
            }

            if (IsClient)
            {
                WeaponAmmoSystem.ClientTryReloadOrSwitchAmmoType(
                    isSwitchAmmoType: false,
                    // no need to send to the server as it will automatically load the necessary ammo of the same type
                    sendToServer: !weaponState.IsFiring);
            }
            else
            {
                WeaponAmmoSystem.ServerTryReloadSameAmmo(character);
            }

            if (weaponState.WeaponReloadingState is null)
            {
                // do not attempt idle auto-reloading again
                weaponState.IsIdleAutoReloadingAllowed = false;
            }

            return false;
        }

        public virtual bool SharedOnFire(ICharacter character, WeaponState weaponState)
        {
            // consume ammo
            if (this.AmmoConsumptionPerShot == 0)
            {
                // not using ammo
                return true;
            }

            var privateState = GetPrivateState(weaponState.ItemWeapon);
            if (privateState.AmmoCount < this.AmmoConsumptionPerShot)
            {
                // not enough ammo
                return false;
            }

            privateState.SetAmmoCount(
                (ushort)(privateState.AmmoCount - this.AmmoConsumptionPerShot));
            return true;
        }

        public virtual void SharedOnHit(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            double damage,
            WeaponHitData hitData,
            out bool isDamageStop)
        {
            isDamageStop = false;

            var protoItemAmmo = weaponCache.Weapon is null
                                    ? null
                                    : GetPrivateState(weaponCache.Weapon).CurrentProtoItemAmmo;

            if (IsServer)
            {
                protoItemAmmo?.ServerOnObjectHit(weaponCache,
                                                 damagedObject,
                                                 damage,
                                                 hitData,
                                                 ref isDamageStop);

                if (damage > 0
                    && damagedObject is ICharacter damagedCharacter)
                {
                    this.ServerTryToApplySpecialEffect(weaponCache, damage, damagedCharacter);
                }
            }
            else // if client
            {
                protoItemAmmo?.ClientOnObjectHit(weaponCache,
                                                 damagedObject,
                                                 hitData,
                                                 ref isDamageStop);
            }
        }

        public virtual void SharedOnMiss(WeaponFinalCache weaponCache, Vector2D endPosition)
        {
            var protoItemAmmo = weaponCache.Weapon is null
                                    ? null
                                    : GetPrivateState(weaponCache.Weapon).CurrentProtoItemAmmo;

            if (protoItemAmmo is null)
            {
                return;
            }

            if (IsServer)
            {
                protoItemAmmo.ServerOnMiss(weaponCache, endPosition);
            }
            else
            {
                protoItemAmmo.ClientOnMiss(weaponCache, endPosition);
            }
        }

        public virtual void SharedOnWeaponAmmoChanged(IItem item, ushort ammoCount)
        {
        }

        public virtual double SharedUpdateAndGetFirePatternCurrentSpreadAngleDeg(WeaponState state)
        {
            var pattern = this.FirePatternPreset;
            if (!pattern.IsEnabled)
            {
                // no weapon fire spread
                return 0;
            }

            state.FirePatternCooldownSecondsRemains = this.FirePatternCooldownDuration;

            var shotNumber = state.FirePatternCurrentShotNumber;
            state.FirePatternCurrentShotNumber = (ushort)(shotNumber + 1);

            var initialSequenceLength = pattern.InitialSequence.Length;
            if (shotNumber < initialSequenceLength)
            {
                // initial fire sequence
                return pattern.InitialSequence[shotNumber];
            }

            // cycled fire sequence
            var sequenceNumber = (shotNumber - initialSequenceLength) % pattern.CycledSequence.Length;
            return pattern.CycledSequence[sequenceNumber];
        }

        protected static ICollection<TAmmo> GetAmmoOfType<TAmmo>()
            where TAmmo : class, IProtoItemAmmo
        {
            return Api.FindProtoEntities<TAmmo>();
        }

        protected static TProtoSkillWeapons GetSkill<TProtoSkillWeapons>()
            where TProtoSkillWeapons : ProtoSkillWeapons, new()
        {
            return Api.GetProtoEntity<TProtoSkillWeapons>();
        }

        protected override void ClientItemHotbarSelectionChanged(ClientHotbarItemData data)
        {
            if (!data.IsSelected)
            {
                this.helperInputListener?.Stop();
                this.helperInputListener = null;
                return;
            }

            // if weapon is selected
            // setup input context and reload it if it's empty
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            this.helperInputListener = ClientInputContext
                                       .Start("Current weapon")
                                       .HandleButtonDown(
                                           GameButton.ItemReload,
                                           () => WeaponAmmoSystem.ClientTryReloadOrSwitchAmmoType(
                                               isSwitchAmmoType: false))
                                       .HandleButtonDown(
                                           GameButton.ItemSwitchMode,
                                           () => WeaponAmmoSystem.ClientTryReloadOrSwitchAmmoType(
                                               isSwitchAmmoType: true));

            if (this.AmmoCapacity == 0)
            {
                // the item is non-reloadable
                return;
            }

            var itemPrivateState = data.Item.GetPrivateState<WeaponPrivateState>();
            if (itemPrivateState.AmmoCount / this.AmmoConsumptionPerShot < 1)
            {
                // the selected weapon is empty (not enough ammo even for a single shot), request reloading
                WeaponAmmoSystem.ClientTryReloadOrSwitchAmmoType(
                    isSwitchAmmoType: false,
                    showNotificationIfNoAmmo: true);
            }
        }

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            WeaponSystem.ClientChangeWeaponFiringMode(isFiring: false);
            // we don't want to play any "item use" sound
            return false;
        }

        protected override void ClientItemUseStart(ClientItemData data)
        {
            WeaponSystem.ClientChangeWeaponFiringMode(isFiring: true);
        }

        protected virtual void ClientPreloadTextures()
        {
            PreloadTextures(this.FireTracePreset);

            foreach (var protoAmmo in this.CompatibleAmmoProtos)
            {
                PreloadTextures(protoAmmo.FireTracePreset);
            }

            void PreloadTextures(WeaponFireTracePreset tracePreset)
            {
                if (tracePreset is null)
                {
                    return;
                }

                var traceTexture = tracePreset.TraceTexture;
                if (traceTexture is not null)
                {
                    Client.Rendering.PreloadTextureAsync(traceTexture);
                }

                tracePreset.HitSparksPreset.PreloadTextures();
            }
        }

        protected override void ClientTooltipCreateControlsInternal(IItem item, List<UIElement> controls)
        {
            base.ClientTooltipCreateControlsInternal(item, controls);

            var hasAmmo = this.CompatibleAmmoProtos.Count > 0;

            if (item is not null
                && hasAmmo)
            {
                controls.Add(ItemTooltipCurrentAmmoControl.Create(item));
            }

            if (this is not IProtoItemTool)
            {
                controls.Add(
                    ItemTooltipWeaponStats.Create(item, this));
            }

            if (hasAmmo)
            {
                controls.Add(new ItemTooltipCompatibleAmmoControl() { ProtoItemWeapon = this });
            }
        }

        protected virtual WeaponFirePatternPreset PrepareFirePatternPreset()
        {
            return default;
        }

        protected virtual WeaponFireScatterPreset PrepareFireScatterPreset()
        {
            return default;
        }

        protected virtual WeaponFireTracePreset PrepareFireTracePreset()
        {
            return default;
        }

        protected override void PrepareProtoItem()
        {
            base.PrepareProtoItem();

            if (this.FireAnimationDuration > this.FireInterval)
            {
                throw new Exception(
                    $"{nameof(this.FireAnimationDuration)} is bigger than {nameof(this.FireInterval)}");
            }

            this.CachedWeaponTextureResource = this.WeaponTextureResource;

            DamageDescription overrideDamageDescription = null;
            this.PrepareProtoWeapon(
                out var compatibleAmmoProtos,
                ref overrideDamageDescription);
            this.SoundPresetWeapon = this.PrepareSoundPresetWeapon();
            this.SoundPresetHit = this.PrepareSoundPresetHit();
            this.FirePatternPreset = this.PrepareFirePatternPreset();
            this.FireScatterPreset = this.PrepareFireScatterPreset();
            this.FireTracePreset = this.PrepareFireTracePreset();

            this.CompatibleAmmoProtos = compatibleAmmoProtos?.Distinct().ToArray()
                                        ?? Array.Empty<IProtoItemAmmo>();

            foreach (var protoItemAmmo in this.CompatibleAmmoProtos)
            {
                protoItemAmmo.PrepareRegisterCompatibleWeapon(this);
            }

            if (this.CompatibleAmmoProtos.Count == 0
                && overrideDamageDescription is null)
            {
                throw new Exception(
                    $"The weapon {this} doesn't have ammo and overrideDamageDescription is null.");
            }

            if (this.AmmoCapacity > 0
                && this.CompatibleAmmoProtos.Count == 0
                && this.FireTracePreset.TraceTexture is null)
            {
                throw new Exception(
                    $"The weapon {this} doesn't have ammo and FireTracePreset is not assigned. Please override: {nameof(this.PrepareFireTracePreset)}().");
            }

            this.OverrideDamageDescription = overrideDamageDescription;

            if (this.DamageApplyDelay >= this.FireInterval
                && this.DamageApplyDelay > 0)
            {
                throw new Exception("DamageApplyDelaySeconds must be lower than FireIntervalSeconds for " + this);
            }

            if (this.SpecialEffectProbability < 0
                || this.SpecialEffectProbability > 1)
            {
                throw new Exception(
                    $"The {nameof(this.SpecialEffectProbability)} property returning an incorrect value: the value must be in [0;1] range.");
            }

            if (this.AmmoCapacity > 0)
            {
                if (this.AmmoCapacity < this.AmmoConsumptionPerShot)
                {
                    throw new Exception(
                        $"{nameof(this.AmmoCapacity)} is less than {nameof(this.AmmoConsumptionPerShot)}");
                }

                if (this.AmmoCapacity % this.AmmoConsumptionPerShot > 0)
                {
                    throw new Exception(nameof(this.AmmoCapacity)
                                        + " cannot be divided without remainder on "
                                        + nameof(this.AmmoConsumptionPerShot)
                                        + " please adjust one or both of the properties accordingly");
                }
            }
            else if (this.AmmoConsumptionPerShot > 0)
            {
                throw new Exception("This weapon doesn't use ammo but overrides property "
                                    + nameof(this.AmmoConsumptionPerShot));
            }
        }

        protected abstract void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription);

        protected abstract ReadOnlySoundPreset<ObjectMaterial> PrepareSoundPresetHit();

        protected abstract ReadOnlySoundPreset<WeaponSound> PrepareSoundPresetWeapon();

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            ItemDurabilitySystem.ServerInitializeItem(data.PrivateState, data.IsFirstTimeInit);
        }

        protected abstract void ServerOnDegradeWeapon(
            ICharacter character,
            IItem weaponItem,
            IProtoItemWeapon protoWeapon,
            IReadOnlyList<IWorldObject> hitObjects);

        protected virtual void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
        }

        private void ServerTryToApplySpecialEffect(
            WeaponFinalCache weaponCache,
            double damage,
            ICharacter damagedCharacter)
        {
            var weapon = weaponCache.Weapon;
            var probability = weaponCache.SpecialEffectProbability;

            //Logger.WriteDev($"Special effect probability: {specialEffectProbability} {this}");

            if (probability >= 1
                || probability > 0 && RandomHelper.RollWithProbability(probability))
            {
                // the special effect has been rolled - try to apply it
                if (weapon is not null)
                {
                    var protoItemAmmo = GetPrivateState(weapon).CurrentProtoItemAmmo;
                    if (protoItemAmmo is null
                        || !protoItemAmmo.IsSuppressWeaponSpecialEffect)
                    {
                        // call special effect for the weapon proto
                        this.ServerOnSpecialEffect(damagedCharacter, damage);
                    }
                }
                else // no weapon item (only the weapon item prototype is available - this)
                {
                    // call special effect for the weapon proto
                    this.ServerOnSpecialEffect(damagedCharacter, damage);
                }
            }
        }
    }
}