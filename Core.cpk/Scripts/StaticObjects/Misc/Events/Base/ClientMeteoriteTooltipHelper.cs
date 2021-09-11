namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Meteorites;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public static class ClientMeteoriteTooltipHelper
    {
        private static readonly IComponentAttachedControl ComponentAttachedUIElement;

        private static readonly MeteoriteTooltipControl MeteoriteTooltipControl;

        private static readonly IClientSceneObject SceneObject;

        static ClientMeteoriteTooltipHelper()
        {
            SceneObject = Api.Client.Scene.CreateSceneObject(nameof(ClientMeteoriteTooltipHelper));
            MeteoriteTooltipControl = new MeteoriteTooltipControl();

            ComponentAttachedUIElement = Api.Client.UI.AttachControl(
                SceneObject,
                MeteoriteTooltipControl,
                positionOffset: (0, 0.25),
                isFocusable: false);

            ComponentAttachedUIElement.IsEnabled = false;
        }

        public static void Refresh(IStaticWorldObject worldObjectMeteorite, bool isObserving)
        {
            if (!isObserving)
            {
                ComponentAttachedUIElement.IsEnabled = false;
                MeteoriteTooltipControl.Setup(null);
                return;
            }

            SceneObject.Position = worldObjectMeteorite.TilePosition.ToVector2D()
                                   + worldObjectMeteorite.ProtoStaticWorldObject
                                                         .SharedGetObjectCenterWorldOffset(worldObjectMeteorite);
            MeteoriteTooltipControl.Setup(worldObjectMeteorite);
            ComponentAttachedUIElement.IsEnabled = true;
        }
    }
}