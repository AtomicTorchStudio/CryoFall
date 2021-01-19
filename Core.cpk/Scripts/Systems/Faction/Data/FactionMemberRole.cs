namespace AtomicTorch.CBND.CoreMod.Systems.Faction
{
    using AtomicTorch.CBND.GameApi;

    [RemoteEnum]
    public enum FactionMemberRole : byte
    {
        Member = 0,

        Officer1 = 1,

        Officer2 = 2,

        Officer3 = 3,

        Leader = byte.MaxValue
    }
}