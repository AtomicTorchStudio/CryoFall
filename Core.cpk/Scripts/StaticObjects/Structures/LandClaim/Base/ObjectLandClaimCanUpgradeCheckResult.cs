namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum ObjectLandClaimCanUpgradeCheckResult : byte
    {
        Success,

        ErrorUnknown,

        [Description("Upgrading will create an intersection with a different person's land claim.")]
        ErrorAreaIntersection,

        [Description("The upgrade requirements are not satisfied.")]
        ErrorRequirementsNotSatisfied,

        [Description("You're not the land claim owner.")]
        ErrorNotFounder,

        [Description("Please reopen the land claim window and try again.")]
        ErrorNoActiveInteraction,

        [Description(LandClaimSystem.ErrorRaidBlockActionRestricted_Message)]
        ErrorUnderRaid,

        [Description(LandClaimSystem.ErrorCannotBuild_DemoPlayerLandClaims)]
        ErrorAreaIntersectionDemoPlayer,

        [Description(LandClaimSystem.ErrorCannotBuild_ExceededSafeStorageCapacity)]
        ErrorExceededSafeStorageCapacity,

        [Description(LandClaimSystem.ErrorCannotBuild_IntersectingWithAnotherLandClaimUnderShieldProtection)]
        ErrorAreaIntersectionWithShieldProtectedArea,

        [Description(CoreStrings.ShieldProtection_Error_CannotUpgradeLandClaimUnderShieldProtection)]
        ErrorUnderShieldProtection,

        /// <summary>
        /// This error happens when player attempts to upgrade a claim that will join a faction-owned base,
        /// and this player has no this faction access rights to do so.
        /// </summary>
        ErrorFactionPermissionRequired,

        ErrorFactionLandClaimNumberLimitWillBeExceeded
    }
}