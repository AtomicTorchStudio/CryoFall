namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectLandClaimPublicState : StaticObjectPublicState
    {
        [SyncToClient]
        public ILogicObject LandClaimAreaObject { get; set; }

        /// <summary>
        /// Server time (in seconds) when the land claim object will be destroyed.
        /// If value is null, the object is not destroyed.
        /// If value is not null, the object will be destroyed when this time is exceeded.
        /// The value must be set to not null time when structure points decreased to 0.
        /// The value must be set to null when structure points recovered to the max amount.
        /// </summary>
        [SyncToClient]
        public double? ServerTimeForDestruction { get; set; }
    }
}