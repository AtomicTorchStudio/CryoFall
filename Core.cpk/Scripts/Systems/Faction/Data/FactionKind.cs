namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum FactionKind : byte
    {
        /// <summary>
        /// Anyone can join freely.
        /// </summary>
        [Description(CoreStrings.Faction_FactionKind_Public_Title)]
        [DescriptionTooltip(CoreStrings.Faction_FactionKind_Public_Description)]
        Public = 0,

        /// <summary>
        /// An application from player should be submitted and accepted by this faction officer.
        /// Applications could be disabled.
        /// </summary>
        [Description(CoreStrings.Faction_FactionKind_Private_Title)]
        [DescriptionTooltip(CoreStrings.Faction_FactionKind_Private_ApplicationOrInvitationRequired
                            + "[br]"
                            + CoreStrings.Faction_FactionKind_Private_Description)]
        Private = 1
    }
}