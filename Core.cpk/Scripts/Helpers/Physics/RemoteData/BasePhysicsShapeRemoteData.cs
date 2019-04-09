namespace AtomicTorch.CBND.CoreMod.Helpers.Physics
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public abstract class BasePhysicsShapeRemoteData : IRemoteCallParameter
    {
        protected BasePhysicsShapeRemoteData(IPhysicsShape shape)
        {
            this.CollisionGroupId = CollisionGroups.GetCollisionGroupId(shape.CollisionGroup);
        }

        public CollisionGroupId CollisionGroupId { get; }
    }
}