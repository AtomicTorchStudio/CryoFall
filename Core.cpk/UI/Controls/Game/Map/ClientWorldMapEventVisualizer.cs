namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Events;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ClientWorldMapEventVisualizer : BaseWorldMapVisualizer
    {
        public const string Notification_ActiveEvent_Finished = "Event finished!";

        public const string Notification_ActiveEvent_TimeRemainingFormat = "Time remaining: {0}";

        public const string Notification_ActiveEvent_Title = "Active event";

        private static readonly IWorldClientService ClientWorld = Api.Client.World;

        private readonly List<(ILogicObject activeEvent, FrameworkElement mapControl)> visualizedSearchAreas
            = new();

        public ClientWorldMapEventVisualizer(WorldMapController worldMapController)
            : base(worldMapController)
        {
            ClientWorld.LogicObjectInitialized += this.LogicObjectInitializedHandler;
            ClientWorld.LogicObjectDeinitialized += this.LogicObjectDeinitializedHandler;

            foreach (var activeEvent in ClientWorld.GetGameObjectsOfProto<ILogicObject, IProtoEvent>())
            {
                this.OnActiveEventAdded(activeEvent);
            }
        }

        protected override void DisposeInternal()
        {
            ClientWorld.LogicObjectInitialized -= this.LogicObjectInitializedHandler;
            ClientWorld.LogicObjectDeinitialized -= this.LogicObjectDeinitializedHandler;

            foreach (var activeEvent in ClientWorld.GetGameObjectsOfProto<ILogicObject, IProtoEvent>())
            {
                this.OnActiveEventRemoved(activeEvent);
            }
        }

        private static string GetTooltipText(ILogicObject activeEvent)
        {
            var timeRemains = ClientWorldEventRegularNotificationManager.CalculateEventTimeRemains(activeEvent);
            var text = ClientWorldEventRegularNotificationManager.GetUpdatedEventNotificationText(activeEvent,
                timeRemains,
                addDescription: true);
            return $"[b]{Notification_ActiveEvent_Title}[/b][br]{text}";
        }

        private void LogicObjectDeinitializedHandler(ILogicObject obj)
        {
            if (obj.ProtoGameObject is IProtoEvent)
            {
                this.OnActiveEventRemoved(obj);
            }
        }

        private void LogicObjectInitializedHandler(ILogicObject obj)
        {
            if (obj.ProtoGameObject is IProtoEvent)
            {
                this.OnActiveEventAdded(obj);
            }
        }

        private void OnActiveEventAdded(ILogicObject activeEvent)
        {
            var timeRemains = ClientWorldEventRegularNotificationManager.CalculateEventTimeRemains(activeEvent);
            if (timeRemains < 10)
            {
                // less than 10 seconds remains - not worth to display it on the map
                return;
            }

            if (activeEvent.ProtoGameObject is IProtoEventWithArea)
            {
                // add a circle for the search area
                var publicState = activeEvent.GetPublicState<EventWithAreaPublicState>();
                var circleRadius = publicState.AreaCircleRadius;
                var circleCanvasPosition = this.WorldToCanvasPosition(publicState.AreaCirclePosition.ToVector2D());

                var control = new WorldMapMarkEvent
                {
                    Width = 2 * circleRadius * WorldMapSectorProvider.WorldTileTextureSize,
                    Height = 2 * circleRadius * WorldMapSectorProvider.WorldTileTextureSize,
                    EllipseColorStroke = Api.Client.UI.GetApplicationResource<Color>("Color6"),
                    EllipseColorStart = Api.Client.UI.GetApplicationResource<Color>("Color4").WithAlpha(0),
                    EllipseColorEnd = Api.Client.UI.GetApplicationResource<Color>("Color4").WithAlpha(0x99)
                };

                Canvas.SetLeft(control, circleCanvasPosition.X - control.Width / 2);
                Canvas.SetTop(control, circleCanvasPosition.Y - control.Height / 2);
                Panel.SetZIndex(control, 1);
                this.AddControl(control, false);
                this.visualizedSearchAreas.Add((activeEvent, control));
                ToolTipServiceExtend.SetToolTip(
                    control,
                    new WorldMapMarkEventTooltip()
                    {
                        Text = GetTooltipText(activeEvent),
                        Icon = Api.Client.UI.GetTextureBrush(
                            ((IProtoEvent)activeEvent.ProtoGameObject).Icon)
                    });
            }

            this.RefreshActiveEventInfo(activeEvent);

            // ensure the map mark would be removed after the timeout
            ClientTimersSystem.AddAction(timeRemains + 1,
                                         () => this.OnActiveEventRemoved(activeEvent));
        }

        private void OnActiveEventRemoved(ILogicObject activeEvent)
        {
            for (var index = 0; index < this.visualizedSearchAreas.Count; index++)
            {
                var entry = this.visualizedSearchAreas[index];
                if (!entry.activeEvent.Equals(activeEvent))
                {
                    continue;
                }

                this.visualizedSearchAreas.RemoveAt(index);
                this.RemoveControl(entry.mapControl);
            }
        }

        private void RefreshActiveEventInfo(ILogicObject activeEvent)
        {
            if (activeEvent.IsDestroyed)
            {
                // the notification will be automatically marked to hide after delay when active event is destroyed
                // (a finished event)
                return;
            }

            this.UpdateEventTooltip(activeEvent);

            // schedule recursive update in a second
            ClientTimersSystem.AddAction(
                1,
                () => this.RefreshActiveEventInfo(activeEvent));
        }

        private void UpdateEventTooltip(ILogicObject activeEvent)
        {
            foreach (var entry in this.visualizedSearchAreas)
            {
                if (!entry.activeEvent.Equals(activeEvent))
                {
                    continue;
                }

                var control = entry.mapControl;
                var formattedTextBlock = (WorldMapMarkEventTooltip)ToolTipServiceExtend.GetToolTip(control);
                formattedTextBlock.Text = GetTooltipText(activeEvent);
            }
        }
    }
}