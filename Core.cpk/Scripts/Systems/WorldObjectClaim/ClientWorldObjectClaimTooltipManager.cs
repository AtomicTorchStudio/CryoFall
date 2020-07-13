namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.WorldObjectClaim;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public static class ClientWorldObjectClaimTooltipManager
    {
        private static readonly IComponentAttachedControl componentCurrentObjectDescription;

        private static readonly WorldObjectClaimTooltip controlCurrentObjectDescription;

        private static readonly IClientSceneObject SceneObjectCurrentTagInfo;

        private static IWorldObject focusedWorldObject;

        static ClientWorldObjectClaimTooltipManager()
        {
            SceneObjectCurrentTagInfo =
                Api.Client.Scene.CreateSceneObject(nameof(ClientWorldObjectClaimTooltipManager));
            controlCurrentObjectDescription = new WorldObjectClaimTooltip();
            componentCurrentObjectDescription = Api.Client.UI.AttachControl(
                SceneObjectCurrentTagInfo,
                controlCurrentObjectDescription,
                positionOffset: default,
                isFocusable: false);
        }

        private static IWorldObject ClientGetCurrentFocusedWorldObject()
        {
            WorldObjectClaimIndicator focusedControl = null;

            var mouseScreenPosition = Api.Client.Input.MouseScreenPosition;
            VisualTreeHelper.HitTest(
                (Visual)Api.Client.UI.LayoutRoot.Parent,
                filterCallback: hitCandidate =>
                                {
                                    if (hitCandidate is WorldObjectClaimIndicator c)
                                    {
                                        focusedControl = c;
                                        return HitTestFilterBehavior.Stop;
                                    }

                                    return HitTestFilterBehavior.Continue;
                                },
                resultCallback: result => HitTestResultBehavior.Stop,
                hitTestParameters: new PointHitTestParameters(
                    new Point(mouseScreenPosition.X, mouseScreenPosition.Y)));

            return focusedControl?.TaggedWorldObject;
        }

        private static void Update()
        {
            focusedWorldObject = ClientGetCurrentFocusedWorldObject();
            UpdateSceneObject();
        }

        private static void UpdateSceneObject()
        {
            if (!(focusedWorldObject is IStaticWorldObject)
                || focusedWorldObject.IsInitialized
                || focusedWorldObject.IsDestroyed)
            {
                SceneObjectCurrentTagInfo.Position = default;
                componentCurrentObjectDescription.IsEnabled = false;
                return;
            }

            var publicState = focusedWorldObject.GetPublicState<IWorldObjectPublicStateWithClaim>();
            var tag = publicState.WorldObjectClaim;
            if (tag is null)
            {
                SceneObjectCurrentTagInfo.Position = default;
                componentCurrentObjectDescription.IsEnabled = false;
                return;
            }

            SceneObjectCurrentTagInfo.Position
                = focusedWorldObject.TilePosition.ToVector2D()
                  + focusedWorldObject.ProtoWorldObject
                                      .SharedGetObjectCenterWorldOffset(focusedWorldObject);

            componentCurrentObjectDescription.IsEnabled = true;
            controlCurrentObjectDescription.Setup(tag);
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                ClientUpdateHelper.UpdateCallback += Update;
            }
        }
    }
}