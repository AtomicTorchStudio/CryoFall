namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors
{
    using System;

    public class ObjectFloorClientState : StaticObjectClientState
    {
        [Obsolete("Remove in A24")]
        public uint LastFloorRendererRefreshFrameNumber { get; set; }
    }
}