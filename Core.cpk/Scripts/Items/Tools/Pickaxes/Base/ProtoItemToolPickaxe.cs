namespace AtomicTorch.CBND.CoreMod.Items.Tools.Pickaxes
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    /// <summary>
    /// Item prototype for axe tool.
    /// </summary>
    public abstract class ProtoItemToolPickaxe
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemWeaponMelee
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemToolMining
        where TPrivateState : WeaponPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        protected ProtoItemToolPickaxe()
        {
            var name = this.GetType().Name;
            this.WeaponTextureResource = new TextureResource(
                "Characters/Tools/" + name,
                isProvidesMagentaPixelPosition: true);
        }

        public abstract double DamageToMinerals { get; }

        public abstract double DamageToNonMinerals { get; }

        public virtual double RangeMax => 1;

        protected sealed override ProtoSkillWeapons WeaponSkill => null;

        protected override TextureResource WeaponTextureResource { get; }

        public virtual double ServerGetDamageToMineral(IStaticWorldObject targetObject)
        {
            return this.DamageToMinerals;
        }

        protected override string GenerateIconPath()
        {
            return "Items/Tools/" + this.GetType().Name;
        }

        protected override void PrepareProtoWeapon(
            out IEnumerable<IProtoItemAmmo> compatibleAmmoProtos,
            ref DamageDescription overrideDamageDescription)
        {
            // no ammo used
            compatibleAmmoProtos = null;

            overrideDamageDescription = new DamageDescription(
                damageValue: this.DamageToNonMinerals,
                armorPiercingCoef: 0,
                finalDamageMultiplier: 1,
                rangeMax: this.RangeMax,
                damageDistribution: new DamageDistribution(DamageType.Impact, proportion: 1));
        }

        protected override void ServerOnSpecialEffect(ICharacter damagedCharacter, double damage)
        {
            ServerWeaponSpecialEffectsHelper.OnPickaxeHit(damagedCharacter, damage);
        }
    }

    /// <summary>
    /// Item prototype for axe tool.
    /// </summary>
    public abstract class ProtoItemToolPickaxe
        : ProtoItemToolPickaxe
            <WeaponPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}