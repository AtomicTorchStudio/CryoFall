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

        public const string Notification_ActiveEvent_TimeStartsIn = "Starts in: {0}";

        public const string Notification_ActiveEvent_Title = "World event";

        private static readonly IWorldClientService ClientWorld = Api.Client.World;

        private readonly List<(ILogicObject worldEvent, FrameworkElement mapControl)> visualizedSearchAreas
            = new();

        public ClientWorldMapEventVisualizer(WorldMapController worldMapController)
            : base(worldMapController)
        {
            ClientWorld.LogicObjectInitialized += this.LogicObjectInitializedHandler;
            ClientWorld.LogicObjectDeinitialized += this.LogicObjectDeinitializedHandler;

            foreach (var worldEvent in ClientWorld.GetGameObjectsOfProto<ILogicObject, IProtoEvent>())
            {
                this.OnWorldEventAdded(worldEvent);
            }
        }

        protected override void DisposeInternal()
        {
            ClientWorld.LogicObjectInitialized -= this.LogicObjectInitializedHandler;
            ClientWorld.LogicObjectDeinitialized -= this.LogicObjectDeinitializedHandler;

            foreach (var worldEvent in ClientWorld.GetGameObjectsOfProto<ILogicObject, IProtoEvent>())
            {
                this.OnWorldEventRemoved(worldEvent);
            }
        }

        private static string GetTooltipText(ILogicObject worldEvent)
        {
            var timeRemains = ClientWorldEventRegularNotificationManager.CalculateEventTimeRemains(worldEvent);
            var text = ClientWorldEventRegularNotificationManager.GetUpdatedEventNotificationText(worldEvent,
                timeRemains,
                addDescription: true);
            return $"[b]{Notification_ActiveEvent_Title}[/b][br]{text}";
        }

        private void LogicObjectDeinitializedHandler(ILogicObject obj)
        {
            if (obj.ProtoGameObject is IProtoEvent)
            {
                this.OnWorldEventRemoved(obj);
            }
        }

        private void LogicObjectInitializedHandler(ILogicObject obj)
        {
            if (obj.ProtoGameObject is IProtoEvent)
            {
                this.OnWorldEventAdded(obj);
            }
        }

        private void OnWorldEventAdded(ILogicObject worldEvent)
        {
            var timeRemains = ClientWorldEventRegularNotificationManager.CalculateEventTimeRemains(worldEvent);
            if (timeRemains < 10)
            {
                // less than 10 seconds remains - not worth to display it on the map
                return;
            }

            if (worldEvent.ProtoGameObject is IProtoEventWithArea)
            {
                // add a circle for the search area
                var publicState = worldEvent.GetPublicState<EventWithAreaPublicState>();
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
                this.visualizedSearchAreas.Add((worldEvent, control));
                ToolTipServiceExtend.SetToolTip(
                    control,
                    new WorldMapMarkEventTooltip()
                    {
                        Text = GetTooltipText(worldEvent),
                        Icon = Api.Client.UI.GetTextureBrush(
                            ((IProtoEvent)worldEvent.ProtoGameObject).Icon)
                    });
            }

            this.RefreshWorldEventInfo(worldEvent);

            // ensure the map mark would be removed after the timeout
            ClientTimersSystem.AddAction(timeRemains + 1,
                                         () => this.OnWorldEventRemoved(worldEvent));
        }

        private void OnWorldEventRemoved(ILogicObject worldEvent)
        {
            for (var index = 0; index < this.visualizedSearchAreas.Count; index++)
            {
                var entry = this.visualizedSearchAreas[index];
                if (!entry.worldEvent.Equals(worldEvent))
                {
                    continue;
                }

                this.visualizedSearchAreas.RemoveAt(index);
                this.RemoveControl(entry.mapControl);
            }
        }

        private void RefreshWorldEventInfo(ILogicObject worldEvent)
        {
            if (worldEvent.IsDestroyed)
            {
                // the notification will be automatically marked to hide after delay when active event is destroyed
                // (a finished event)
                return;
            }

            this.UpdateEventTooltip(worldEvent);

            // schedule recursive update in a second
            ClientTimersSystem.AddAction(
                1,
                () => this.RefreshWorldEventInfo(worldEvent));
        }

        private void UpdateEventTooltip(ILogicObject worldEvent)
        {
            foreach (var entry in this.visualizedSearchAreas)
            {
                if (!entry.worldEvent.Equals(worldEvent))
                {
                    continue;
                }

                var control = entry.mapControl;
                var formattedTextBlock = (WorldMapMarkEventTooltip)ToolTipServiceExtend.GetToolTip(control);
                formattedTextBlock.Text = GetTooltipText(worldEvent);
            }
        }
    }
}