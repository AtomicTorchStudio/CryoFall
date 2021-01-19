namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using System.ComponentModel;
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum FactionOfficerRoleTitle : byte
    {
        [Description("Deputy")]
        Deputy = 0,

        [Description("Recruiter")]
        Recruiter = 1,

        [Description("Diplomat")]
        Diplomat = 2,

        [Description("Treasurer")]
        Treasurer = 3,

        [Description("Officer")]
        Officer = 4,

        [Description("Manager")]
        Manager = 5,

        [Description("Minister")]
        Minister = 6
    }
}