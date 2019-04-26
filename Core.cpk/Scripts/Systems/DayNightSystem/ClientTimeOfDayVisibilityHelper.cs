namespace AtomicTorch.CBND.CoreMod.Systems.DayNightSystem
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientTimeOfDayVisibilityHelper
    {
        public static bool ClientIsObservable(IWorldObject worldObject)
        {
            if (!DayNightSystem.IsNight)
            {
                return true;
            }

            if (ClientComponentLightingRenderer.AdditionalAmbientLight > 0
                || ClientComponentLightingRenderer.AdditionalAmbightLightAdditiveFraction > 0)
            {
                // assume night vision or artificial retina implant
                return true;
            }

            // it's night, perform the distance check to closest light source
            Vector2D position;
            switch (worldObject)
            {
                case IDynamicWorldObject dynamicWorldObject:
                    position = dynamicWorldObject.Position;
                    break;
                case IStaticWorldObject staticWorldObject:
                    position = staticWorldObject.TilePosition.ToVector2D()
                               + staticWorldObject.ProtoStaticWorldObject.Layout.Center;
                    break;
                default:
                    position = worldObject.TilePosition.ToVector2D();
                    break;
            }

            foreach (var lightSource in ClientLightSourceManager.AllLightSources)
            {
                var distanceToLightSqr = position.DistanceSquaredTo(lightSource.SceneObject.Position);
                if (distanceToLightSqr <= lightSource.LogicalLightRadiusSqr)
                {
                    // lighted object
                    return true;
                }
            }

            return false;
        }
    }
}