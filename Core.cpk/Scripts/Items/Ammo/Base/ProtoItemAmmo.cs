namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
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
        where TPrivateState : BasePrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        protected ProtoItemAmmo()
        {
            this.Icon = new TextureResource("Items/Ammo/" + this.GetType().Name);
        }

        public DamageDescription DamageDescription { get; private set; }

        public WeaponFireTracePreset FireTracePreset { get; private set; }

        public sealed override ITextureResource Icon { get; }

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
            double damage,
            WeaponHitData hitData,
            ref bool isDamageStop)
        {
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

        protected abstract void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution);

        protected abstract WeaponFireTracePreset PrepareFireTracePreset();

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
        : ProtoItemAmmo<EmptyPrivateState, EmptyPublicState, EmptyClientState>
    {
    }
}