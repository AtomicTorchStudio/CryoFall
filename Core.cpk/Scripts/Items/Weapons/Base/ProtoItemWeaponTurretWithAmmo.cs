namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using AtomicTorch.CBND.CoreMod.Characters.Turrets;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Weapons;

    public abstract class ProtoItemWeaponTurretWithAmmo : ProtoItemWeaponTurret
    {
        public void ServerUpdateCurrentAmmo(ICharacter character, WeaponState weaponState)
        {
            var ammoItem = character.GetPrivateState<CharacterTurretPrivateState>()
                                    .CurrentAmmoItem;
            if (ammoItem is null
                || ammoItem.Count == 0)
            {
                return;
            }

            var protoAmmo = (IProtoItemAmmo)ammoItem.ProtoGameObject;
            if (weaponState.WeaponCache?.ProtoAmmo == protoAmmo)
            {
                return;
            }

            DamageDescription damageDescription;

            if (this.OverrideDamageDescription is not null)
            {
                damageDescription = this.OverrideDamageDescription;
            }
            else if (protoAmmo is IAmmoWithCustomWeaponCacheDamageDescription customAmmo)
            {
                damageDescription = customAmmo.DamageDescriptionForWeaponCache;
            }
            else
            {
                damageDescription = protoAmmo.DamageDescription;
            }

            //Logger.Dev("Switched ammo type: " + weaponState.WeaponCache?.ProtoAmmo + " -> " + protoAmmo);
            WeaponSystem.SharedBuildWeaponCache(character, weaponState, protoAmmo, damageDescription);
        }

        public override bool SharedCanFire(ICharacter character, WeaponState weaponState)
        {
            var ammoItem = character.GetPrivateState<CharacterTurretPrivateState>()
                                    .CurrentAmmoItem;

            if (ammoItem is null
                || ammoItem.Count == 0)
            {
                return false;
            }

            var protoAmmo = (IProtoItemAmmo)ammoItem.ProtoGameObject;
            return weaponState.WeaponCache.ProtoAmmo == protoAmmo;
        }

        public override bool SharedOnFire(ICharacter character, WeaponState weaponState)
        {
            var ammoItem = character.GetPrivateState<CharacterTurretPrivateState>()
                                    .CurrentAmmoItem;
            if (ammoItem is null
                || ammoItem.Count == 0)
            {
                return false;
            }

            Server.Items.SetCount(ammoItem, ammoItem.Count - 1);
            return true;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);
            Logger.Dev("Server update: " + data.GameObject);
        }
    }
}