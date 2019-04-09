namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Timer;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientWorldMapLandClaimVisualizer : IDisposable, IWorldMapVisualizer
    {
        public const string NotificationBaseUnderAttack_MessageFormat
            = @"One of your bases is being raided!
  [br]({0};{1})";

        public const string NotificationBaseUnderAttack_Title = "Under attack!";

        private readonly Dictionary<ILogicObject, LandClaimMapData> visualizedAreas
            = new Dictionary<ILogicObject, LandClaimMapData>();

        private readonly WorldMapController worldMapController;

        public ClientWorldMapLandClaimVisualizer(WorldMapController worldMapController)
        {
            this.worldMapController = worldMapController;

            ClientLandClaimAreaManager.AreaAdded += this.AreaAddedHandler;
            ClientLandClaimAreaManager.AreaRemoved += this.AreaRemovedHandler;

            foreach (var area in ClientLandClaimAreaManager.EnumerateAreaObjects())
            {
                this.AreaAddedHandler(area);
            }
        }

        public bool IsEnabled { get; set; }

        public void Dispose()
        {
            ClientLandClaimAreaManager.AreaAdded -= this.AreaAddedHandler;
            ClientLandClaimAreaManager.AreaRemoved -= this.AreaRemovedHandler;

            if (this.visualizedAreas.Count > 0)
            {
                foreach (var visualizedArea in this.visualizedAreas.ToList())
                {
                    this.AreaRemovedHandler(visualizedArea.Key);
                }
            }
        }

        private void AreaAddedHandler(ILogicObject area)
        {
            if (!LandClaimSystem.ClientIsOwnedArea(area))
            {
                return;
            }

            if (this.visualizedAreas.ContainsKey(area))
            {
                Api.Logger.Error("Land claim area already has the map visualizer: " + area);
                return;
            }

            this.visualizedAreas[area] = new LandClaimMapData(area, this.worldMapController);
        }

        private void AreaRemovedHandler(ILogicObject area)
        {
            if (!this.visualizedAreas.TryGetValue(area, out var control))
            {
                return;
            }

            this.visualizedAreas.Remove(area);
            control.Dispose();
        }

        private class LandClaimMapData : IDisposable
        {
            private readonly ILogicObject area;

            private readonly LandClaimAreaPrivateState areaPrivateState;

            private readonly WorldMapController worldMapController;

            private bool isRaided;

            private FrameworkElement markControl;

            private FrameworkElement markRaidNotificationControl;

            private StateSubscriptionStorage stateSubscriptionStorage;

            public LandClaimMapData(ILogicObject area, WorldMapController worldMapController)
            {
                this.area = area;
                this.areaPrivateState = area.GetPrivateState<LandClaimAreaPrivateState>();
                this.worldMapController = worldMapController;
                this.stateSubscriptionStorage = new StateSubscriptionStorage();

                // add land claim mark control to map
                this.markControl = new WorldMapMarkLandClaim();
                var canvasPosition = this.GetAreaCanvasPosition();
                Canvas.SetLeft(this.markControl, canvasPosition.X);
                Canvas.SetTop(this.markControl, canvasPosition.Y);
                Panel.SetZIndex(this.markControl, 12);

                worldMapController.AddControl(this.markControl);

                this.areaPrivateState.ClientSubscribe(
                    o => o.LastRaidTime,
                    this.RefreshRaidedState,
                    this.stateSubscriptionStorage);

                this.RefreshRaidedState();
            }

            protected bool IsDisposed => this.markControl == null;

            public void Dispose()
            {
                if (this.markControl != null)
                {
                    this.worldMapController.RemoveControl(this.markControl);
                    this.markControl = null;
                }

                this.RemoveRaidNotificationControl();

                this.stateSubscriptionStorage?.Dispose();
                this.stateSubscriptionStorage = null;
            }

            private Vector2D GetAreaCanvasPosition()
            {
                return this.worldMapController.WorldToCanvasPosition(this.GetAreaWorldPosition());
            }

            private Vector2D GetAreaWorldPosition()
            {
                var bounds = LandClaimSystem.SharedGetLandClaimAreaBounds(this.area);
                return (bounds.X + bounds.Width / 2.0,
                        bounds.Y + bounds.Height / 2.0);
            }

            private void RaidRefreshTimerCallback()
            {
                if (this.IsDisposed
                    || !this.isRaided)
                {
                    return;
                }

                this.RefreshRaidedState();

                if (this.isRaided)
                {
                    // still raided
                    ClientComponentTimersManager.AddAction(delaySeconds: 1,
                                                           this.RaidRefreshTimerCallback);
                }
            }

            private void RefreshRaidedState()
            {
                if (!this.areaPrivateState.LastRaidTime.HasValue)
                {
                    this.RemoveRaidNotificationControl();
                    return;
                }

                var timeSinceRaid = Api.Client.CurrentGame.ServerFrameTimeRounded
                                    - this.areaPrivateState.LastRaidTime.Value;
                var isRaidedNow = timeSinceRaid < LandClaimSystem.RaidCooldownDurationSeconds;

                if (this.isRaided == isRaidedNow)
                {
                    // no changes
                    return;
                }

                this.isRaided = isRaidedNow;
                if (!this.isRaided)
                {
                    this.RemoveRaidNotificationControl();
                    return;
                }

                var bounds = LandClaimSystem.SharedGetLandClaimAreaBounds(this.area);
                var x = bounds.X + bounds.Width / 2;
                var y = bounds.Y + bounds.Height / 2;

                // add raid mark control to map
                this.markRaidNotificationControl = new WorldMapMarkRaid();
                var canvasPosition = this.GetAreaCanvasPosition();
                Canvas.SetLeft(this.markRaidNotificationControl, canvasPosition.X);
                Canvas.SetTop(this.markRaidNotificationControl, canvasPosition.Y);
                Panel.SetZIndex(this.markRaidNotificationControl, 11);

                this.worldMapController.AddControl(this.markRaidNotificationControl);

                // show text notification
                NotificationSystem.ClientShowNotification(
                    NotificationBaseUnderAttack_Title,
                    string.Format(NotificationBaseUnderAttack_MessageFormat, x, y),
                    NotificationColor.Bad,
                    icon: Api.GetProtoEntity<ObjectCharredGround>().Icon);

                // start refresh timer
                ClientComponentTimersManager.AddAction(1, this.RaidRefreshTimerCallback);
            }

            private void RemoveRaidNotificationControl()
            {
                this.isRaided = false;
                if (this.markRaidNotificationControl == null)
                {
                    return;
                }

                this.worldMapController.RemoveControl(this.markRaidNotificationControl);
                this.markRaidNotificationControl = null;
            }
        }
    }
}