namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public interface IObjectPublicStateWithActiveFlag : IPublicState
    {
        bool IsActive { get; set; }
    }
}