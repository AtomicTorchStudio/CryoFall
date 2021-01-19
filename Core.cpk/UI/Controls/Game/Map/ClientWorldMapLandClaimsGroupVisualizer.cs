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

    public class ClientWorldMapLandClaimsGroupVisualizer : BaseWorldMapVisualizer
    {
        public const string NotificationBaseUnderAttack_MessageFormat
            = @"One of your bases is being raided!
  [br]({0};{1})";

        public const string NotificationBaseUnderAttack_Title = "Under attack!";

        private static readonly IWorldClientService World = Api.Client.World;

        private readonly Dictionary<ILogicObject, Controller> groupControllers
            = new();

        public ClientWorldMapLandClaimsGroupVisualizer(WorldMapController worldMapController)
            : base(worldMapController)
        {
            World.LogicObjectInitialized += this.LogicObjectInitializedHandler;
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

            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(area);
            if (areasGroup is null)
            {
                return;
            }

            if (!this.groupControllers.TryGetValue(areasGroup, out controller))
            {
                controller = new Controller(areasGroup, this);
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

        protected override void DisposeInternal()
        {
            World.LogicObjectInitialized -= this.LogicObjectInitializedHandler;

            foreach (var controller in this.groupControllers)
            {
                controller.Value.Dispose();
            }

            this.groupControllers.Clear();
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

            private readonly ClientWorldMapLandClaimsGroupVisualizer visualizer;

            private bool isRaided;

            private FrameworkElement markRaidControl;

            private HudNotificationControl raidNotification;

            private IStateSubscriptionOwner stateSubscriptionStorage;

            public Controller(
                ILogicObject areasGroup,
                ClientWorldMapLandClaimsGroupVisualizer visualizer)
            {
                this.areasGroup = areasGroup;
                this.visualizer = visualizer;
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

            private Vector2Ushort GetAreasGroupCanvasPosition(Vector2Ushort worldPosition)
            {
                return this.visualizer
                           .WorldToCanvasPosition(worldPosition.ToVector2D())
                           .ToVector2Ushort();
            }

            private Vector2Ushort GetAreasGroupWorldPosition()
            {
                return LandClaimSystem.SharedGetLandClaimGroupCenterPosition(this.areasGroup);
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
                this.visualizer.AddControl(this.markRaidControl);

                // show notification message
                var localPosition = worldPosition
                                    - Api.Client.World.WorldBounds.Offset;

                this.raidNotification = NotificationSystem.ClientShowNotification(
                    NotificationBaseUnderAttack_Title,
                    string.Format(NotificationBaseUnderAttack_MessageFormat,
                                  WorldMapSectorHelper.GetSectorCoordinateTextForRelativePosition(localPosition)
                                  + "-"
                                  + localPosition.X,
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

                this.visualizer.RemoveControl(this.markRaidControl);
                this.markRaidControl = null;
            }
        }
    }
}