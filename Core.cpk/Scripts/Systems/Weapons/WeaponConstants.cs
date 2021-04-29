namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class WeaponConstants
    {
        public static readonly double DamageCreaturesMultiplier;

        public static readonly double DamageExplosivesToCharactersMultiplier;

        public static readonly double DamageExplosivesToStructuresMultiplier;

        public static readonly double DamageFriendlyFireMultiplier;

        public static readonly double DamagePveMultiplier;

        public static readonly double DamagePvpMultiplier;

        static WeaponConstants()
        {
            DamageCreaturesMultiplier = ServerRates.Get(
                "DamageCreaturesMultiplier",
                defaultValue: 1.0,
                @"All damage dealt by creatures (to player and/or other creatures) is multiplied by this rate.
                  It allows to make it harder or easier to kill players by creatures.");

            DamageExplosivesToCharactersMultiplier = ServerRates.Get(
                "DamageExplosivesToCharactersMultiplier",
                defaultValue: 1.0,
                @"All damage dealt by bombs to characters is multiplied by this rate.
                  You can set it to 0 to disable bomb/grenade damage to characters.
                  Please note that in PvE it's always not possible to damage other characters
                  unless the duel mode is explicitly enabled by players.");

            DamageExplosivesToStructuresMultiplier = ServerRates.Get(
                "DamageExplosivesToStructuresMultiplier",
                defaultValue: 1.0,
                @"All damage dealt by bombs and grenades to structures is multiplied by this rate.
                  You can set it to 0 to disable explosives damage to structures.
                  Applies only on PvP servers, on PvE it will always be 0.");

            DamageFriendlyFireMultiplier = MathHelper.Clamp(
                ServerRates.Get(
                    "DamageFriendlyFireMultiplier",
                    defaultValue: 1.0,
                    @"Multiplier for the friendly fire damage
                      (when a player damaging friendly player (same party/faction, or an ally)
                       with any weapon except explosives).
                      0.0 - disable friendly fire (no damage).
                      1.0 - enable friendly fire (full damage).
                      You can also set it to something in between like 0.5
                      to reduce the damage but not eliminate it completely."),
                min: 0.0,
                max: 1.0);

            DamagePveMultiplier = ServerRates.Get(
                "DamagePveMultiplier",
                defaultValue: 1.0,
                @"All damage dealt from player to environment (NPC/creatures, world objects,
                  also trees and rocks when player is not using a woodcutting/mining tool)
                  is multiplied by this rate.
                  It allows to make it harder or easier to kill creatures by players.");

            DamagePvpMultiplier = ServerRates.Get(
                "DamagePvpMultiplier",
                defaultValue: 0.5,
                @"All damage dealt from player to player (via weapons only) is multiplied by this rate.
                  It allows to decrease or increase the combat duration.
                  You can set it to 0 to disable PvP damage (it doesn't apply to bombs damage).");
        }
    }
}