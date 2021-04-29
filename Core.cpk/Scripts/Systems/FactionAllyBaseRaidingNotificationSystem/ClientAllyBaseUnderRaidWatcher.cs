namespace AtomicTorch.CBND.CoreMod.Systems.FactionAllyBaseRaidingNotificationSystem
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.CoreMod.Items.Explosives.Bombs;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public static class ClientAllyBaseUnderRaidWatcher
    {
        private static readonly Dictionary<uint, IHudNotificationControl> Notifications = new();

        static ClientAllyBaseUnderRaidWatcher()
        {
            FactionAllyBaseRaidingNotificationSystem.ClientAllyBasesUnderRaid
                                                    .CollectionChanged
                += AllyBasesUnderRaidCollectionChangedHandler;

            foreach (var data in FactionAllyBaseRaidingNotificationSystem.ClientAllyBasesUnderRaid)
            {
                Register(data);
            }
        }

        private static void AllyBasesUnderRaidCollectionChangedHandler(
            object sender,
            NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                Reset();
                return;
            }

            if (e.OldItems is not null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    Unregister((FactionAllyBaseRaidingNotificationSystem.ClientAllyBaseUnderRaidMark)oldItem);
                }
            }

            if (e.NewItems is not null)
            {
                foreach (var newItem in e.NewItems)
                {
                    Register((FactionAllyBaseRaidingNotificationSystem.ClientAllyBaseUnderRaidMark)newItem);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        private static void EnsureInitialized()
        {
        }

        private static void Register(FactionAllyBaseRaidingNotificationSystem.ClientAllyBaseUnderRaidMark data)
        {
            var areasGroupId = data.LandClaimAreasGroupId;
            if (Notifications.TryGetValue(areasGroupId, out var notification))
            {
                throw new Exception("Already registered: " + data);
            }

            var positionText = WorldMapSectorHelper.FormatWorldPositionWithSectorCoordinate(data.WorldPosition);
            var title = ClientWorldMapAllyBaseUnderRaidVisualizer.NotificationRaid_Title;
            var message = ClientWorldMapAllyBaseUnderRaidVisualizer.GetNotificationMessage(data);
            message += "[br]" + positionText;

            notification = NotificationSystem.ClientShowNotification(
                title,
                message,
                NotificationColor.Bad,
                autoHide: false,
                icon: Api.GetProtoEntity<ItemBombModern>().Icon);

            Notifications[areasGroupId] = notification;
        }

        private static void Reset()
        {
            foreach (var notification in Notifications.Values)
            {
                notification.Hide(quick: true);
            }

            Notifications.Clear();
        }

        private static void Unregister(FactionAllyBaseRaidingNotificationSystem.ClientAllyBaseUnderRaidMark data)
        {
            var areasGroupId = data.LandClaimAreasGroupId;
            if (!Notifications.TryGetValue(areasGroupId, out var notification))
            {
                return;
            }

            Notifications.Remove(areasGroupId);
            notification.Hide(quick: true);
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                EnsureInitialized();
            }
        }
    }
}