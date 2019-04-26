namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using AtomicTorch.GameEngine.Common.Helpers;

    public static class WeaponConstants
    {
        public static readonly double DamageCreaturesMultiplier
            = ServerRates.Get(
                "DamageCreaturesMultiplier",
                defaultValue: 1.0,
                @"All damage dealt by creatures (to player and/or other creatures)
                  is multiplied on this rate.
                  It allows to make it harder or easier to kill players by creatures.");

        public static readonly double DamageExplosivesToCharactersMultiplier
            = ServerRates.Get(
                "DamageExplosivesToCharactersMultiplier",
                defaultValue: 1.0,
                @"All damage dealt by bombs to characters is multiplied on this rate.
                  You can set it to 0 to disable bombs damage to characters.");

        public static readonly double DamageExplosivesToStructuresMultiplier
            = ServerRates.Get(
                "DamageExplosivesToStructuresMultiplier",
                defaultValue: 1.0,
                @"All damage dealt by bombs to structures is multiplied on this rate.
                  You can set it to 0 to disable bombs damage to structures.");

        public static readonly double DamageFriendlyFireMultiplier
            = MathHelper.Clamp(
                ServerRates.Get(
                    "DamageFriendlyFireMultiplier",
                    defaultValue: 0.0,
                    @"Multiplier for the friendly fire damage
                      (when one party member damaging another with any weapon except explosives).
                      0.0 - disable friendly fire completely.
                      1.0 - enable friendly fire completely.
                      You can also set it to something in between like 0.5
                      to reduce the damage but not eliminate it completely."),
                min: 0.0,
                max: 1.0);

        public static readonly double DamagePveMultiplier
            = ServerRates.Get(
                "DamagePveMultiplier",
                defaultValue: 1.0,
                @"All damage dealt from player to environment (NPC/creatures, world objects,
                  also trees and rocks when player is not using a woodcutting/mining tool)
                  is multiplied on this rate.
                  It allows to make it harder or easier to kill creatures by players.");

        public static readonly double DamagePvpMultiplier
            = ServerRates.Get(
                "DamagePvpMultiplier",
                defaultValue: 0.5,
                @"All damage dealt from player to player is multiplied on this rate.
                  It allows to decrease or increase the combat duration.
                  You can set it to 0 to disable PvP damage (doesn't apply to bombs damage!).
                  Default value on A21 version is 0.5.");
    }
}