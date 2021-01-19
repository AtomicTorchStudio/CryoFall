namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    [Flags]
    public enum WorldObjectFactionAccessModes : byte
    {
        Closed = 0,

        Leader = 1 << 0,

        Officer1 = 1 << 1,

        Officer2 = 1 << 2,

        Officer3 = 1 << 3,

        [Description("Opens to all faction members")]
        AllFactionMembers = Leader | Officer1 | Officer2 | Officer3 | 1 << 4,

        [Description("Opens to ally faction members")]
        AllyFactionMembers = AllFactionMembers | 1 << 7,

        Everyone = byte.MaxValue
    }
}