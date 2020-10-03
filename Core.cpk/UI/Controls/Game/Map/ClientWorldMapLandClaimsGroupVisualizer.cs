namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Items.Explosives.Bombs;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientWorldMapLandClaimsGroupVisualizer : IWorldMapVisualizer
    {
        public const string NotificationBaseUnderAttack_MessageFormat
            = @"One of your bases is being raided!
  [br]({0};{1})";

        public const string NotificationBaseUnderAttack_Title = "Under attack!";

        private static readonly IWorldClientService World = Api.Client.World;

        private readonly Dictionary<ILogicObject, Controller> groupControllers
            = new Dictionary<ILogicObject, Controller>();

        private readonly WorldMapController worldMapController;

        public ClientWorldMapLandClaimsGroupVisualizer(WorldMapController worldMapController)
        {
            this.worldMapController = worldMapController;
            World.LogicObjectInitialized += this.LogicObjectInitializedHandler;
        }

        public bool IsEnabled { get; set; }

        public void Dispose()
        {
            World.LogicObjectInitialized -= this.LogicObjectInitializedHandler;

            foreach (var controller in this.groupControllers)
            {
                controller.Value.Dispose();
            }

            this.groupControllers.Clear();
        }

        public void Register(ILogicObject area)
        {
            Controller controller;
            foreach (var pair in this.groupControllers)
            {
                controller = pair.Value;
                if (controller.Areas.Contains(area))
                {
                    throw new Exception("Already has area registered: " + area);
                }
            }

            var areasGroup = LandClaimArea.GetPublicState(area).LandClaimAreasGroup;
            if (areasGroup is null)
            {
                return;
            }

            if (!this.groupControllers.TryGetValue(areasGroup, out controller))
            {
                controller = new Controller(areasGroup, this.worldMapController);
                this.groupControllers[areasGroup] = controller;
            }

            controller.Areas.Add(area);
        }

        public void Unregister(ILogicObject area)
        {
            foreach (var pair in this.groupControllers)
            {
                var controller = pair.Value;
                if (!controller.Areas.Contains(area))
                {
                    continue;
                }

                controller.Areas.Remove(area);
                if (controller.Areas.Count == 0)
                {
                    this.groupControllers.Remove(pair.Key);
                    controller.Dispose();
                }

                return;
            }
        }

        private void LogicObjectInitializedHandler(ILogicObject logicObject)
        {
            if (this.groupControllers.TryGetValue(logicObject, out var controller))
            {
                controller.ReInitialize();
            }
        }

        private class Controller
        {
            public readonly IList<ILogicObject> Areas = new List<ILogicObject>();

            private readonly ILogicObject areasGroup;

            private readonly WorldMapController worldMapController;

            private bool isRaided;

            private FrameworkElement markRaidControl;

            private HudNotificationControl raidNotification;

            private IStateSubscriptionOwner stateSubscriptionStorage;

            public Controller(
                ILogicObject areasGroup,
                WorldMapController worldMapController)
            {
                this.areasGroup = areasGroup;
                this.worldMapController = worldMapController;
                this.ReInitialize();
            }

            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                if (this.IsDisposed)
                {
                    return;
                }

                this.IsDisposed = true;
                this.Cleanup();
            }

            public void ReInitialize()
            {
                this.Cleanup();

                this.stateSubscriptionStorage = new StateSubscriptionStorage();

                var groupPublicState = LandClaimAreasGroup.GetPublicState(this.areasGroup);
                groupPublicState.ClientSubscribe(
                    o => o.LastRaidTime,
                    this.RefreshRaidedState,
                    this.stateSubscriptionStorage);

                this.RefreshRaidedState();
            }

            private void Cleanup()
            {
                this.RemoveRaidNotification();

                this.stateSubscriptionStorage?.Dispose();
                this.stateSubscriptionStorage = null;
            }

            private Vector2Ushort GetAreasGroupCanvasPosition(Vector2D worldPosition)
            {
                return this.worldMapController.WorldToCanvasPosition(worldPosition)
                           .ToVector2Ushort();
            }

            private Vector2D GetAreasGroupWorldPosition()
            {
                var bounds = LandClaimSystem.SharedGetLandClaimGroupsBoundingArea(this.areasGroup);
                return (bounds.X + bounds.Width / 2.0,
                        bounds.Y + bounds.Height / 2.0);
            }

            private void RaidRefreshTimerCallback()
            {
                if (this.IsDisposed)
                {
                    return;
                }

                this.RefreshRaidedState();

                if (this.isRaided)
                {
                    // still raided
                    ClientTimersSystem.AddAction(delaySeconds: 1,
                                                 this.RaidRefreshTimerCallback);
                }
            }

            private void RefreshRaidedState()
            {
                var isRaidedNow = LandClaimSystem.SharedIsAreasGroupUnderRaid(this.areasGroup);
                if (this.isRaided == isRaidedNow)
                {
                    // no changes
                    return;
                }

                // remove outdated notifications (if exists)
                this.RemoveRaidNotification();

                this.isRaided = isRaidedNow;
                if (!this.isRaided)
                {
                    return;
                }

                // was not raided, is raided now
                var worldPosition = this.GetAreasGroupWorldPosition();
                // add raid mark control to map
                this.markRaidControl = new WorldMapMarkRaid();
                var canvasPosition = this.GetAreasGroupCanvasPosition(worldPosition);
                Canvas.SetLeft(this.markRaidControl, canvasPosition.X);
                Canvas.SetTop(this.markRaidControl, canvasPosition.Y);
                Panel.SetZIndex(this.markRaidControl, 11);
                this.worldMapController.AddControl(this.markRaidControl);

                // show notification message
                var localPosition = worldPosition.ToVector2Ushort()
                                    - Api.Client.World.WorldBounds.Offset;

                this.raidNotification = NotificationSystem.ClientShowNotification(
                    NotificationBaseUnderAttack_Title,
                    string.Format(NotificationBaseUnderAttack_MessageFormat,
                                  localPosition.X,
                                  localPosition.Y),
                    NotificationColor.Bad,
                    autoHide: false,
                    icon: Api.GetProtoEntity<ItemBombModern>().Icon);

                // start refresh timer
                ClientTimersSystem.AddAction(delaySeconds: 1,
                                             this.RaidRefreshTimerCallback);
            }

            private void RemoveRaidNotification()
            {
                this.raidNotification?.Hide(quick: true);
                this.raidNotification = null;

                if (this.markRaidControl is null)
                {
                    return;
                }

                this.worldMapController.RemoveControl(this.markRaidControl);
                this.markRaidControl = null;
            }
        }
    }
}