namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode
{
    using System.ComponentModel;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.GameEngine.Common.Extensions;

    [RemoteEnum]
    public enum WorldObjectDirectAccessMode : byte
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

        /// <summary>
        /// Used only for doors and specific crates (such as an armored safe).
        /// </summary>
        [Description("Always closed")]
        [DescriptionOrder(3)]
        Closed = byte.MaxValue
    }
}