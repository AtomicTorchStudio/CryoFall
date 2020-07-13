namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.UI;

    public enum ObjectLandClaimCanUpgradeCheckResult
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
        ErrorUnderShieldProtection
    }
}