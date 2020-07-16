namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public static class ServerMobWeaponHelper
    {
        public static void TrySetWeapon(
            ICharacter character,
            IProtoItemWeapon protoWeapon,
            bool rebuildWeaponsCacheNow)
        {
            var privateState = character.GetPrivateState<CharacterMobPrivateState>();
            var publicState = character.GetPublicState<CharacterMobPublicState>();
            var weaponState = privateState.WeaponState;

            if (ReferenceEquals(weaponState.ProtoWeapon, protoWeapon))
            {
                return;
            }

            if (weaponState.CooldownSecondsRemains > 0.001
                || weaponState.DamageApplyDelaySecondsRemains > 0.001)
            {
                //Api.Logger.Dev("Weapon cooldown remains: " + weaponState.CooldownSecondsRemains);
                return;
            }

            weaponState.SharedSetWeaponProtoOnly(protoWeapon);
            publicState.SharedSetCurrentWeaponProtoOnly(protoWeapon);
            
            // can use the new selected mob weapon instantly
            weaponState.ReadySecondsRemains = weaponState.CooldownSecondsRemains = 0;

            if (!rebuildWeaponsCacheNow)
            {
                return;
            }

            WeaponSystem.SharedRebuildWeaponCache(character, weaponState);
            privateState.AttackRange = weaponState.WeaponCache.RangeMax;
        }
    }
}