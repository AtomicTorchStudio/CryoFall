namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;

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
        ErrorUnderRaid
    }
}