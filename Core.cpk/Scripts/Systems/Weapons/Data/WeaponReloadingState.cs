namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class WeaponReloadingState
    {
        public readonly IItem Item;

        public readonly IProtoItemAmmo ProtoItemAmmo;

        public double SecondsToReloadRemains;

        public WeaponReloadingState(
            ICharacter character,
            IItem item,
            IProtoItemWeapon itemProto,
            IProtoItemAmmo protoItemAmmo)
        {
            this.Item = item;
            this.ProtoItemAmmo = protoItemAmmo;

            this.SecondsToReloadRemains = itemProto.AmmoReloadDuration;

            var statName = itemProto.WeaponSkillProto?.StatNameReloadingSpeedMultiplier;
            if (statName.HasValue)
            {
                this.SecondsToReloadRemains *= character.SharedGetFinalStatMultiplier(statName.Value);
            }

            //Api.Logger.WriteDev($"Weapon will be reloaded in: {this.SecondsToReloadRemains:F2} seconds");
        }
    }
}