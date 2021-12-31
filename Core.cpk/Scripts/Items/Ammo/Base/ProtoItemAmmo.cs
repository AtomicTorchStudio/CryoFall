namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Damage;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Options.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoItemAmmo
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItem
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemAmmo
        where TPrivateState : ItemPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        private readonly List<IProtoItemWeapon> listCompatibleWeaponProtos = new();

        public IReadOnlyList<IProtoItemWeapon> CompatibleWeaponProtos
            => this.listCompatibleWeaponProtos;

        public DamageDescription DamageDescription { get; private set; }

        public virtual DamageStatsComparisonPreset DamageStatsComparisonPreset
            => DamageStatsComparisonPresets.PresetRangedExceptGrenades;

        public WeaponFireTracePreset FireTracePreset { get; private set; }

        public abstract bool IsReferenceAmmo { get; }

        public virtual bool IsSuppressWeaponSpecialEffect => false;

        /// <summary>
        /// Gets the item stack size.
        /// </summary>
        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public virtual WeaponFireScatterPreset? OverrideFireScatterPreset { get; }

        public virtual void ClientOnMiss(WeaponFinalCache weaponCache, Vector2D endPosition)
        {
        }

        public virtual void ClientOnObjectHit(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            WeaponHitData hitData,
            ref bool isDamageStop)
        {
        }

        public void PrepareRegisterCompatibleWeapon(IProtoItemWeapon protoItemWeapon)
        {
            this.listCompatibleWeaponProtos.Add(protoItemWeapon);
        }

        public virtual void ServerOnCharacterHit(ICharacter damagedCharacter, double damage, ref bool isDamageStop)
        {
        }

        public virtual void ServerOnMiss(WeaponFinalCache weaponCache, Vector2D endPosition)
        {
        }

        public virtual void ServerOnObjectHit(
            WeaponFinalCache weaponCache,
            IWorldObject damagedObject,
            double damage,
            WeaponHitData hitData,
            ref bool isDamageStop)
        {
            if (damagedObject is ICharacter damagedCharacter)
            {
                this.ServerOnCharacterHit(damagedCharacter,
                                          damage,
                                          ref isDamageStop);
            }
        }

        protected override void ClientTooltipCreateControlsInternal(IItem item, List<UIElement> controls)
        {
            base.ClientTooltipCreateControlsInternal(item, controls);

            if (this.DamageDescription.DamageValue > 0)
            {
                controls.Add(
                    ItemTooltipInfoDamageDescription.Create(
                        this.DamageDescription,
                        this.OverrideFireScatterPreset,
                        damageMultiplier: 1,
                        rangeMultiplier: 1,
                        comparisonPreset: this.DamageStatsComparisonPreset,
                        displayRange: false));
            }

            controls.Add(
                ItemTooltipCompatibleWeaponsControl.Create(this));
        }

        protected abstract void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution);

        protected abstract WeaponFireTracePreset PrepareFireTracePreset();

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);

            if (this.CompatibleWeaponProtos
                    .SelectMany(w => w.CompatibleAmmoProtos)
                    .Distinct()
                    .Count()
                > 1)
            {
                // there are other similar ammo types,
                // add a hint explaining how to switch the loaded ammo type 
                var key = ClientInputManager.GetKeyForButton(GameButton.ItemSwitchMode);
                hints.Add(string.Format(ItemHints.AmmoSwitch_Format,
                                        InputKeyNameHelper.GetKeyText(key)));
            }
        }

        protected override void PrepareProtoItem()
        {
            base.PrepareProtoItem();

            var damageDistribution = new DamageDistribution();
            this.FireTracePreset = this.PrepareFireTracePreset();

            this.PrepareDamageDescription(
                out var damageValue,
                out var armorPiercingCoef,
                out var finalDamageMultiplier,
                out var rangeMax,
                damageDistribution);

            this.DamageDescription = new DamageDescription(
                damageValue,
                armorPiercingCoef,
                finalDamageMultiplier,
                rangeMax,
                damageDistribution);
        }
    }

    public abstract class ProtoItemAmmo
        : ProtoItemAmmo
            <ItemPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}