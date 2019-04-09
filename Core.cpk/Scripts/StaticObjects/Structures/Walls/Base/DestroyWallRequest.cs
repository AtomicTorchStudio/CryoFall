namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls
{
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    internal class DestroyWallRequest : IRemoteCallParameter
    {
        public readonly bool IsForce;

        public DestroyWallRequest(IStaticWorldObject wallObject, bool isForce)
        {
            this.WallObject = wallObject;
            this.IsForce = isForce;
        }

        public DestroyWallRequest()
        {
        }

        public IStaticWorldObject WallObject { get; }
    }
}