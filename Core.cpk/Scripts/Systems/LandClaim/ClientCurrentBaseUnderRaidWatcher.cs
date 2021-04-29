namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Items.Explosives.Bombs;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public static class ClientCurrentBaseUnderRaidWatcher
    {
        private static readonly Dictionary<ILogicObject, Controller> GroupControllers = new();

        private static readonly IWorldClientService World = Api.Client.World;

        static ClientCurrentBaseUnderRaidWatcher()
        {
            World.LogicObjectInitialized += LogicObjectInitializedHandler;
            ClientLandClaimAreaManager.AreaAdded += AreaAddedHandler;
            ClientLandClaimAreaManager.AreaRemoved += AreaRemovedHandler;
        }

        private static void AreaAddedHandler(ILogicObject area)
        {
            Register(area);
        }

        private static void AreaRemovedHandler(ILogicObject area)
        {
            Unregister(area);
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static void EnsureInitialized()
        {
        }

        private static void LogicObjectInitializedHandler(ILogicObject logicObject)
        {
            if (GroupControllers.TryGetValue(logicObject, out var controller))
            {
                controller.ReInitialize();
            }
        }

        private static void Register(ILogicObject area)
        {
            if (!LandClaimSystem.ClientIsOwnedArea(area, requireFactionPermission: false))
            {
                return;
            }

            Controller controller;
            foreach (var pair in GroupControllers)
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

            if (!GroupControllers.TryGetValue(areasGroup, out controller))
            {
                controller = new Controller(areasGroup);
                GroupControllers[areasGroup] = controller;
            }

            controller.Areas.Add(area);
        }

        private static void Unregister(ILogicObject area)
        {
            foreach (var pair in GroupControllers)
            {
                var controller = pair.Value;
                if (!controller.Areas.Contains(area))
                {
                    continue;
                }

                controller.Areas.Remove(area);
                if (controller.Areas.Count == 0)
                {
                    GroupControllers.Remove(pair.Key);
                    controller.Dispose();
                }

                return;
            }
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                EnsureInitialized();
            }
        }

        private class Controller
        {
            public readonly IList<ILogicObject> Areas = new List<ILogicObject>();

            private readonly ILogicObject areasGroup;

            private bool lastIsRaided;

            private HudNotificationControl raidNotification;

            private IStateSubscriptionOwner stateSubscriptionStorage;

            public Controller(ILogicObject areasGroup)
            {
                this.areasGroup = areasGroup;
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
                this.Reset();
            }

            public void ReInitialize()
            {
                this.Reset();

                this.stateSubscriptionStorage = new StateSubscriptionStorage();

                var groupPublicState = LandClaimAreasGroup.GetPublicState(this.areasGroup);
                groupPublicState.ClientSubscribe(
                    o => o.LastRaidTime,
                    this.RefreshRaidedState,
                    this.stateSubscriptionStorage);

                this.RefreshRaidedState();
            }

            private void RefreshRaidedState()
            {
                var isRaidedNow = LandClaimSystem.SharedIsAreasGroupUnderRaid(this.areasGroup);
                if (this.lastIsRaided == isRaidedNow)
                {
                    // no changes
                    return;
                }

                // remove outdated notifications (if exists)
                this.RemoveRaidNotification();

                this.lastIsRaided = isRaidedNow;
                if (!this.lastIsRaided)
                {
                    return;
                }

                // was not raided, is raided now
                var worldPosition = LandClaimSystem.SharedGetLandClaimGroupCenterPosition(this.areasGroup);
                var positionText = WorldMapSectorHelper.FormatWorldPositionWithSectorCoordinate(worldPosition);
                var title = ClientWorldMapLandClaimsGroupVisualizer.NotificationBaseUnderAttack_Title;
                var message = ClientWorldMapLandClaimsGroupVisualizer.NotificationBaseUnderAttack_Message;
                message += "[br]" + positionText;

                this.raidNotification = NotificationSystem.ClientShowNotification(
                    title,
                    message,
                    NotificationColor.Bad,
                    autoHide: false,
                    icon: Api.GetProtoEntity<ItemBombModern>().Icon);
            }

            private void RemoveRaidNotification()
            {
                this.lastIsRaided = false;
                this.raidNotification?.Hide(quick: true);
                this.raidNotification = null;
            }

            private void Reset()
            {
                this.RemoveRaidNotification();

                this.stateSubscriptionStorage?.Dispose();
                this.stateSubscriptionStorage = null;
            }
        }
    }
}