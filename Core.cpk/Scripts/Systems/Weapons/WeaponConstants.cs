namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi.Scripting;
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
                @"All damage dealt by creatures (to player and/or other creatures) is multiplied on this rate.
                  It allows to make it harder or easier to kill players by creatures.");

            DamageExplosivesToCharactersMultiplier = ServerRates.Get(
                "DamageExplosivesToCharactersMultiplier",
                defaultValue: 1.0,
                @"All damage dealt by bombs to characters is multiplied on this rate.
                  You can set it to 0 to disable bombs damage to characters.
                  Applies only on PvP servers - on PvE it will be always 0.");

            DamageExplosivesToStructuresMultiplier = ServerRates.Get(
                "DamageExplosivesToStructuresMultiplier",
                defaultValue: 1.0,
                @"All damage dealt by bombs to structures is multiplied on this rate.
                  You can set it to 0 to disable bombs damage to structures.
                  Applies only on PvP servers - on PvE it will be always 0.");

            DamageFriendlyFireMultiplier = MathHelper.Clamp(
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

            DamagePveMultiplier = ServerRates.Get(
                "DamagePveMultiplier",
                defaultValue: 1.0,
                @"All damage dealt from player to environment (NPC/creatures, world objects,
                  also trees and rocks when player is not using a woodcutting/mining tool)
                  is multiplied on this rate.
                  It allows to make it harder or easier to kill creatures by players.");

            DamagePvpMultiplier = ServerRates.Get(
                "DamagePvpMultiplier",
                defaultValue: 0.5,
                @"All damage dealt from player to player (via weapons only) is multiplied on this rate.
                  It allows to decrease or increase the combat duration.
                  You can set it to 0 to disable PvP damage (doesn't apply to bombs damage!).");

            if (!Api.IsServer
                || PveSystem.ServerIsPvE)
            {
                // set explosives rates to zero for PvE servers and all clients
                DamageExplosivesToCharactersMultiplier = 0;
                DamageExplosivesToStructuresMultiplier = 0;
            }
        }
    }
}