namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;

    /// <summary>
    /// A special interface to tag static objects that are spawned during the events.
    /// It's used to restrict construction too close to them.
    /// </summary>
    [NotPersistent]
    [NotNetworkAvailable]
    public interface IProtoObjectEventEntry : IProtoStaticWorldObject
    {
    }
}