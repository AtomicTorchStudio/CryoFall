namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Systems.FactionAllyBaseRaidingNotificationSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientWorldMapAllyBaseUnderRaidVisualizer : BaseWorldMapVisualizer
    {
        public const string NotificationRaid_AllyBase_MessageFormat
            = "A base of an allied faction \\[{0}\\] is being raided!";

        public const string NotificationRaid_FactionMember_MessageFormat
            = "A private base of your faction's member @{0} is being raided!";

        public const string NotificationRaid_Title = "Ally base is under attack!";

        private readonly Dictionary<uint, Controller> controllers = new();

        public ClientWorldMapAllyBaseUnderRaidVisualizer(WorldMapController worldMapController)
            : base(worldMapController)
        {
            FactionAllyBaseRaidingNotificationSystem.ClientAllyBasesUnderRaid
                                                    .CollectionChanged
                += this.AllyBasesUnderRaidCollectionChangedHandler;

            foreach (var data in FactionAllyBaseRaidingNotificationSystem.ClientAllyBasesUnderRaid)
            {
                this.Register(data);
            }
        }

        public static string GetNotificationMessage(
            FactionAllyBaseRaidingNotificationSystem.ClientAllyBaseUnderRaidMark data)
        {
            return string.IsNullOrEmpty(data.ClanTag)
                       ? string.Format(NotificationRaid_FactionMember_MessageFormat,
                                       data.FactionMemberName)
                       : string.Format(NotificationRaid_AllyBase_MessageFormat,
                                       data.ClanTag);
        }

        protected override void DisposeInternal()
        {
            FactionAllyBaseRaidingNotificationSystem.ClientAllyBasesUnderRaid
                                                    .CollectionChanged
                -= this.AllyBasesUnderRaidCollectionChangedHandler;

            this.Reset();
        }

        private void AllyBasesUnderRaidCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.Reset();
                return;
            }

            if (e.OldItems is not null)
            {
                foreach (var oldItem in e.OldItems)
                {
                    this.Unregister((FactionAllyBaseRaidingNotificationSystem.ClientAllyBaseUnderRaidMark)oldItem);
                }
            }

            if (e.NewItems is not null)
            {
                foreach (var newItem in e.NewItems)
                {
                    this.Register((FactionAllyBaseRaidingNotificationSystem.ClientAllyBaseUnderRaidMark)newItem);
                }
            }
        }

        private void Register(FactionAllyBaseRaidingNotificationSystem.ClientAllyBaseUnderRaidMark data)
        {
            var areasGroupId = data.LandClaimAreasGroupId;
            if (this.controllers.TryGetValue(areasGroupId, out var controller))
            {
                throw new Exception("Already registered: " + data);
            }

            controller = new Controller(this, data);
            this.controllers[areasGroupId] = controller;
        }

        private void Reset()
        {
            foreach (var controller in this.controllers)
            {
                controller.Value.Reset();
            }

            this.controllers.Clear();
        }

        private void Unregister(FactionAllyBaseRaidingNotificationSystem.ClientAllyBaseUnderRaidMark data)
        {
            var areasGroupId = data.LandClaimAreasGroupId;
            if (!this.controllers.TryGetValue(areasGroupId, out var controller))
            {
                return;
            }

            this.controllers.Remove(areasGroupId);
            controller.Reset();
        }

        private class Controller
        {
            private readonly ClientWorldMapAllyBaseUnderRaidVisualizer visualizer;

            private FrameworkElement markRaidControl;

            public Controller(
                ClientWorldMapAllyBaseUnderRaidVisualizer visualizer,
                FactionAllyBaseRaidingNotificationSystem.ClientAllyBaseUnderRaidMark data)
            {
                this.visualizer = visualizer;

                var notificationTitle = NotificationRaid_Title;
                var notificationMessage = GetNotificationMessage(data);

                // add raid mark control to map
                this.markRaidControl = new WorldMapMarkRaid()
                {
                    Description = "[b]" + notificationTitle + "[/b][br]" + notificationMessage
                };

                var canvasPosition = this.GetAreasGroupCanvasPosition(data.WorldPosition);
                Canvas.SetLeft(this.markRaidControl, canvasPosition.X);
                Canvas.SetTop(this.markRaidControl, canvasPosition.Y);
                Panel.SetZIndex(this.markRaidControl, 11);
                this.visualizer.AddControl(this.markRaidControl);
            }

            public void Reset()
            {
                this.visualizer.RemoveControl(this.markRaidControl);
                this.markRaidControl = null;
            }

            private Vector2Ushort GetAreasGroupCanvasPosition(Vector2Ushort worldPosition)
            {
                return this.visualizer
                           .WorldToCanvasPosition(worldPosition.ToVector2D())
                           .ToVector2Ushort();
            }
        }
    }
}