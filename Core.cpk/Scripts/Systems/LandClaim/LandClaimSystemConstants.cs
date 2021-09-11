namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class LandClaimSystemConstants
    {
        static LandClaimSystemConstants()
        {
            RateAdditionalLandClaimsNumber.ClientValueChanged
                += ClientRateAdditionalLandClaimsNumberChanged;
        }

        public static ushort SharedLandClaimsNumberLimitIncrease
            => RateAdditionalLandClaimsNumber.GetSharedValue(logErrorIfClientHasNoValue: false);

        public static double SharedRaidBlockDurationSeconds
            => RatePvPRaidBlockDuration.GetSharedValue(logErrorIfClientHasNoValue: false);

        private static void ClientRateAdditionalLandClaimsNumberChanged()
        {
            Api.GetProtoEntity<PlayerCharacter>()
               .SharedReinitializeDefaultEffects();

            Api.Client.Characters.CurrentPlayerCharacter
               .SharedSetFinalStatsCacheDirty();
        }
    }
}