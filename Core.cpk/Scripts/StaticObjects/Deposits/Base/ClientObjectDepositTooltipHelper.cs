namespace AtomicTorch.CBND.CoreMod.StaticObjects.Deposits
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public static class ClientObjectDepositTooltipHelper
    {
        private static readonly IComponentAttachedControl ComponentAttachedUIElement;

        private static readonly DepositTooltipControl DepositTooltipControl;

        private static readonly IClientSceneObject SceneObject;

        static ClientObjectDepositTooltipHelper()
        {
            SceneObject = Api.Client.Scene.CreateSceneObject(nameof(ClientObjectDepositTooltipHelper));
            DepositTooltipControl = new DepositTooltipControl();

            ComponentAttachedUIElement = Api.Client.UI.AttachControl(
                SceneObject,
                DepositTooltipControl,
                positionOffset: (0, 0),
                isFocusable: false);

            ComponentAttachedUIElement.IsEnabled = false;
        }

        public static void Refresh(IStaticWorldObject worldObjectDeposit, bool isObserving)
        {
            if (!isObserving)
            {
                ComponentAttachedUIElement.IsEnabled = false;
                DepositTooltipControl.Setup(null);
                return;
            }

            SceneObject.Position = worldObjectDeposit.TilePosition.ToVector2D()
                                   + worldObjectDeposit.ProtoStaticWorldObject.Layout.Center;
            DepositTooltipControl.Setup(worldObjectDeposit);
            ComponentAttachedUIElement.IsEnabled = true;
        }
    }
}