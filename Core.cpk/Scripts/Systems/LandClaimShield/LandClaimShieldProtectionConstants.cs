namespace AtomicTorch.CBND.CoreMod.Systems.LandClaimShield
{
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public static class LandClaimShieldProtectionConstants
    {
        static LandClaimShieldProtectionConstants()
        {
            if (Api.IsClient)
            {
                return;
            }

            SharedIsEnabled =
                !PveSystem.ServerIsPvE
                && ServerRates.Get(
                    "PvP.ShieldProtection.Enabled",
                    defaultValue: 1,
                    @"Set it to 0 to disable the S.H.I.E.L.D. protection (PvP only).
                    Set it to 1 to enable the S.H.I.E.L.D. protection.")
                > 0;

            SharedCooldownDuration = ServerRates.Get(
                "PvP.ShieldProtection.CooldownDuration",
                defaultValue: 30 * 60,
                @"Cannot reactivate a deactivated shield for this duration (in seconds).");

            SharedActivationDuration = ServerRates.Get(
                "PvP.ShieldProtection.ActivationDuration",
                defaultValue: 15 * 60,
                @"Shield activation duration (in seconds).");
        }

        /// <summary>
        /// Shield activation duration.
        /// </summary>
        public static double SharedActivationDuration { get; private set; }

        /// <summary>
        /// Cannot reactivate a deactivated shield for this duration.
        /// </summary>
        public static double SharedCooldownDuration { get; private set; }

        public static bool SharedIsEnabled { get; private set; }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static void EnsureInitialized()
        {
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += Refresh;
                Refresh();

                static async void Refresh()
                {
                    if (Api.Client.Characters.CurrentPlayerCharacter is null)
                    {
                        return;
                    }

                    (SharedIsEnabled, SharedActivationDuration, SharedCooldownDuration) =
                        await LandClaimShieldProtectionSystem.Instance.CallServer(_ => _.ServerRemote_GetSettings());
                }
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                EnsureInitialized();
            }
        }
    }
}