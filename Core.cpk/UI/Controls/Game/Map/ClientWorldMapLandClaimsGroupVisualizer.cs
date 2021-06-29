namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientWorldMapLandClaimsGroupVisualizer : BaseWorldMapVisualizer
    {
        public const string NotificationBaseUnderAttack_Message
            = "One of your bases is being raided!";

        public const string NotificationBaseUnderAttack_Title = "Under attack!";

        private static readonly IWorldClientService World = Api.Client.World;

        private readonly Dictionary<ILogicObject, Controller> groupControllers
            = new();

        public ClientWorldMapLandClaimsGroupVisualizer(WorldMapController worldMapController)
            : base(worldMapController)
        {
            World.LogicObjectInitialized += this.LogicObjectInitializedHandler;
            ClientUpdateHelper.UpdateCallback += this.Update;
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
                controller = new Controller(this, areasGroup);
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
            ClientUpdateHelper.UpdateCallback -= this.Update;

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

        private void Update()
        {
            foreach (var controller in this.groupControllers)
            {
                // updating is necessary as the raided state cannot be tracked
                // entirely by the client subscription on a property in the public state
                controller.Value.RefreshRaidedState();
            }
        }

        private class Controller
        {
            public readonly IList<ILogicObject> Areas = new List<ILogicObject>();

            private readonly ILogicObject areasGroup;

            private readonly ClientWorldMapLandClaimsGroupVisualizer visualizer;

            private bool lastIsRaided;

            private FrameworkElement markRaidControl;

            private IStateSubscriptionOwner stateSubscriptionStorage;

            public Controller(
                ClientWorldMapLandClaimsGroupVisualizer visualizer,
                ILogicObject areasGroup)
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
                if (this.IsDisposed)
                {
                    return;
                }

                this.Cleanup();

                this.stateSubscriptionStorage = new StateSubscriptionStorage();

                var groupPublicState = LandClaimAreasGroup.GetPublicState(this.areasGroup);
                groupPublicState.ClientSubscribe(
                    o => o.LastRaidTime,
                    this.RefreshRaidedState,
                    this.stateSubscriptionStorage);

                this.RefreshRaidedState();
            }

            internal void RefreshRaidedState()
            {
                var isRaidedNow = LandClaimSystem.SharedIsAreasGroupUnderRaid(this.areasGroup);
                if (this.lastIsRaided == isRaidedNow)
                {
                    // no changes
                    return;
                }

                // remove outdated notifications (if exists)
                this.RemoveControl();

                this.lastIsRaided = isRaidedNow;
                if (!this.lastIsRaided)
                {
                    return;
                }

                // was not raided, is raided now
                var worldPosition = this.GetAreasGroupWorldPosition();
                var notificationTitle = NotificationBaseUnderAttack_Title;
                var notificationMessage = string.Format(NotificationBaseUnderAttack_Message);

                // add raid mark control to map
                this.markRaidControl = new WorldMapMarkRaid()
                {
                    Description = "[b]" + notificationTitle + "[/b][br]" + notificationMessage
                };

                var canvasPosition = this.GetAreasGroupCanvasPosition(worldPosition);
                Canvas.SetLeft(this.markRaidControl, canvasPosition.X);
                Canvas.SetTop(this.markRaidControl, canvasPosition.Y);
                Panel.SetZIndex(this.markRaidControl, 11);
                this.visualizer.AddControl(this.markRaidControl);
            }

            private void Cleanup()
            {
                this.RemoveControl();

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

            private void RemoveControl()
            {
                this.lastIsRaided = false;
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