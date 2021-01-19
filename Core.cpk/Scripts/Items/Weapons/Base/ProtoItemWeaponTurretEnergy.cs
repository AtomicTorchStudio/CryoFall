namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public abstract class ProtoItemWeaponTurretEnergy : ProtoItemWeaponTurret
    {
        /// <summary>
        /// Determines the amount of base electricity to deduct per shot.
        /// </summary>
        public abstract double EnergyUsagePerShot { get; }

        public override bool SharedCanFire(ICharacter character, WeaponState weaponState)
        {
            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(character.TilePosition);
            return areasGroup is not null
                   && PowerGridSystem.ServerBaseHasCharge(areasGroup, this.EnergyUsagePerShot);
        }

        public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
        {
            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(character.TilePosition);
            if (areasGroup is null)
            {
                return false;
            }

            PowerGridSystem.ServerDeductBaseCharge(areasGroup, this.EnergyUsagePerShot);
            return true;
        }
    }
}