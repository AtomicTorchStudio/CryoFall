namespace AtomicTorch.CBND.CoreMod.Drones
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Trees;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class DroneTargetPositionHelper
    {
        public static Vector2D GetTargetPosition(IStaticWorldObject targetWorldObject)
        {
            var protoStaticWorldObject = targetWorldObject.ProtoStaticWorldObject;
            var centerOffset = protoStaticWorldObject.SharedGetObjectCenterWorldOffset(targetWorldObject);

            if (protoStaticWorldObject is IProtoObjectTree)
            {
                centerOffset = (centerOffset.X, 1.0);
            }

            return centerOffset;
        }
    }
}