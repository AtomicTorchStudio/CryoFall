namespace AtomicTorch.CBND.CoreMod.Items.Ammo
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Resources;

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

        public sealed override ITextureResource Icon { get; }

        public virtual bool IsSuppressWeaponSpecialEffect => false;

        /// <summary>
        /// Gets the item stack size.
        /// </summary>
        public override ushort MaxItemsPerStack => ItemStackSize.Medium;

        public virtual void ServerOnCharacterHit(ICharacter damagedCharacter, double damage)
        {
        }

        protected abstract void PrepareDamageDescription(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            out double rangeMax,
            DamageDistribution damageDistribution);

        protected override void PrepareProtoItem()
        {
            base.PrepareProtoItem();
            var damageDistribution = new DamageDistribution();

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