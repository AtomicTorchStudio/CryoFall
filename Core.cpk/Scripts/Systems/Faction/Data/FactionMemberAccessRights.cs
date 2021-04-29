namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;

    /// <summary>
    /// This enum used to determine faction member access rights
    /// as well as perform access right checks by using bitwise
    /// operators or HasFlag method.
    /// </summary>
    [RemoteEnum]
    [Flags]
    public enum FactionMemberAccessRights : ushort
    {
        None = 0,

        [Description("Edit faction description")]
        EditDescription = 1 << 0,

        [Description("Recruitment")]
        Recruitment = 1 << 1,

        [Description("Assign member roles")]
        SetMemberRole = 1 << 2,

        [Description("Remove members")]
        RemoveMembers = 1 << 3,

        [Description("Diplomacy management")]
        DiplomacyManagement = 1 << 4,

        [Description(CoreStrings.Faction_Permission_LandClaimManagement_Title)]
        [DescriptionTooltip(CoreStrings.Faction_Permission_LandClaimManagement_Tooltip)]
        LandClaimManagement = 1 << 5,

        [Description("Vehicle access management")]
        VehicleAccessManagement = 1 << 6,

        [Description("Base S.H.I.E.L.D. management")]
        BaseShieldManagement = 1 << 7,

        /// <summary>
        /// Special entry for the leader (full access rights).
        /// </summary>
        Leader = ushort.MaxValue
    }
}