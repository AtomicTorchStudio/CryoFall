namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ObjectMineralPragmiumHelper
    {
        public static ClientComponentSpriteLightSource ClientInitializeLightForNode(IStaticWorldObject worldObject)
        {
            return ClientLighting.CreateLightSourceSpot(
                Api.Client.Scene.GetSceneObject(worldObject),
                color: LightColors.PragmiumLuminescenceNode,
                size: 5,
                spritePivotPoint: (0.5, 0.5),
                positionOffset: (0.5, 0.55));
        }

        public static ClientComponentSpriteLightSource ClientInitializeLightForSource(IStaticWorldObject worldObject)
        {
            return ClientLighting.CreateLightSourceSpot(
                Api.Client.Scene.GetSceneObject(worldObject),
                color: LightColors.PragmiumLuminescenceSource,
                size: 14,
                spritePivotPoint: (0.5, 0.5),
                positionOffset: (1, 1.35));
        }
    }
}