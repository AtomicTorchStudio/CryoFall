namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode
{
    using System.ComponentModel;
    using AtomicTorch.GameEngine.Common.Extensions;

    public enum WorldObjectAccessMode : byte
    {
        [Description("Opens to owners")]
        [DescriptionOrder(0)]
        OpensToObjectOwners = 0,

        [Description("Opens to owners or land owners")]
        [DescriptionOrder(1)]
        OpensToObjectOwnersOrAreaOwners = 10,

        [Description("Opens to everyone")]
        [DescriptionOrder(2)]
        OpensToEveryone = 20,

        [Description("Always closed")]
        [DescriptionOrder(3)]
        Closed = byte.MaxValue
    }
}