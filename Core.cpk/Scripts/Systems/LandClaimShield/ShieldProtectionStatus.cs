namespace AtomicTorch.CBND.CoreMod.Systems.LandClaimShield
{
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.UI;

    public enum ShieldProtectionStatus : byte
    {
        [Description(CoreStrings.ShieldProtection_Status_Inactive)]
        Inactive = 0,

        [Description(CoreStrings.ShieldProtection_Status_Activating)]
        Activating = 1,

        [Description(CoreStrings.ShieldProtection_Status_Active)]
        Active = 2
    }
}