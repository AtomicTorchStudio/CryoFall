namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ConsoleCommands.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public partial class WindowWorldMap : BaseWindowMenu
    {
        public const string ContextMenuCopyCoordinates = "Copy coordinates";

        public const string ContextMenuTeleport = "Teleport";

        private readonly List<BaseWorldMapVisualizer> visualizers = new();

        private ControlWorldMap controlWorldMap;

        public Action<Vector2Ushort> MapClickOverride { get; set; }

        public IEnumerable<BaseWorldMapVisualizer> Visualizers => this.visualizers;

        public WorldMapController WorldMapController => this.controlWorldMap.WorldMapController;

        public void AddVisualizer(BaseWorldMapVisualizer visualizer)
        {
            this.visualizers.Add(visualizer);
            visualizer.IsEnabled = this.WorldMapController.IsActive;
        }

        public void RemoveVisualizer(BaseWorldMapVisualizer visualizer)
        {
            visualizer.IsEnabled = false;
            this.visualizers.Remove(visualizer);
        }

        protected override void InitMenu()
        {
            base.InitMenu();
            this.controlWorldMap = this.GetByName<ControlWorldMap>("ControlWorldMap");
        }

        protected override void OnLoaded()
        {
            this.controlWorldMap.Loaded += this.ControlWorldMapLoadedHandler;
            this.controlWorldMap.MouseRightButtonUp += this.ControlWorldMapMouseRightButtonUpHandler;

            if (this.controlWorldMap.IsLoaded)
            {
                this.ControlWorldMapLoadedHandler(null, null);
            }
        }

        protected override void OnUnloaded()
        {
            this.controlWorldMap.Loaded -= this.ControlWorldMapLoadedHandler;
            this.controlWorldMap.MouseRightButtonUp -= this.ControlWorldMapMouseRightButtonUpHandler;

            this.DestroyVisualizers();
        }

        protected override void WindowClosed()
        {
            if (this.controlWorldMap.WorldMapController is not null)
            {
                this.controlWorldMap.WorldMapController.IsActive = false;
            }

            foreach (var visualizer in this.visualizers)
            {
                visualizer.IsEnabled = false;
            }

            base.WindowClosed();
        }

        protected override void WindowClosing()
        {
            base.WindowClosing();
            this.CloseContextMenu();
        }

        protected override void WindowOpening()
        {
            this.TryActivateWorldMapController();
            base.WindowOpening();
        }

        private static async void CallCreativeModeTeleport(Vector2D worldPosition)
        {
            var message = await ConsolePlayerTeleport.ClientCallTeleportAsync(worldPosition);

            const string title = ContextMenuTeleport;
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            const string errorPrefix = "Error: ";

            if (message.StartsWith(errorPrefix, StringComparison.Ordinal))
            {
                message = message.Substring(errorPrefix.Length);
                NotificationSystem.ClientShowNotification(title, message, color: NotificationColor.Bad);
            }
            else
            {
                NotificationSystem.ClientShowNotification(title, message);
            }
        }

        private void CloseContextMenu()
        {
            if (this.ContextMenu is not null)
            {
                this.ContextMenu.IsOpen = false;
                this.ContextMenu = null;
            }
        }

        private void ControlWorldMapLoadedHandler(object sender, RoutedEventArgs e)
        {
            var controller = this.controlWorldMap.WorldMapController;
            if (controller is null)
            {
                throw new InvalidOperationException();
            }

            this.RestoreDefaultVisualizers();

            this.TryActivateWorldMapController();
            controller.CenterMapOnPlayerCharacter(true);
        }

        private void ControlWorldMapMouseRightButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            this.CloseContextMenu();

            var controller = this.controlWorldMap.WorldMapController;
            if (this.MapClickOverride is not null)
            {
                var mapPositionAbsolute = controller.PointedMapWorldPositionAbsolute;
                Api.SafeInvoke(() => this.MapClickOverride(mapPositionAbsolute));
                return;
            }

            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem()
            {
                Header = ContextMenuCopyCoordinates,
                Command = new ActionCommand(
                    () => Api.Client.Core.CopyToClipboard(
                        WorldMapSectorHelper.FormatWorldPositionWithSectorCoordinate(
                            controller.PointedMapWorldPositionAbsolute)))
            });

            var character = Api.Client.Characters.CurrentPlayerCharacter;
            if (character.ProtoCharacter is PlayerCharacterSpectator
                || CreativeModeSystem.SharedIsInCreativeMode(character)
                || Api.IsEditor)
            {
                var mapPositionWithoutOffset = controller.PointedMapWorldPositionAbsolute;
                contextMenu.Items.Add(new MenuItem()
                {
                    Header = ContextMenuTeleport,
                    Command = new ActionCommand(
                        () => CallCreativeModeTeleport(mapPositionWithoutOffset.ToVector2D()))
                });
            }

            var position = Mouse.GetPosition(this);
            contextMenu.PlacementRectangle = new Rect(position.X, position.Y, 0, 0);

            this.ContextMenu = contextMenu;
            contextMenu.Closed += ContextMenuOnClosed;
            contextMenu.IsOpen = true;

            void ContextMenuOnClosed(object s, RoutedEventArgs _)
            {
                contextMenu.Closed -= ContextMenuOnClosed;
                // remove context menu with the delay (to avoid teleport-on-click when context menu is closed)
                ClientTimersSystem.AddAction(
                    delaySeconds: 0.1,
                    () =>
                    {
                        if (this.ContextMenu == contextMenu)
                        {
                            this.ContextMenu = null;
                        }
                    });
            }
        }

        private void DestroyVisualizers()
        {
            if (this.visualizers.Count == 0)
            {
                return;
            }

            foreach (var visualizer in this.visualizers)
            {
                try
                {
                    visualizer.Dispose();
                }
                catch (Exception ex)
                {
                    Api.Logger.Exception(ex, "Exception during visualizer disposing");
                }
            }

            this.visualizers.Clear();
        }

        private void MapClickHandler(Vector2D worldPosition)
        {
            if (this.ContextMenu is not null)
            {
                // context menu is still exist, don't process this click
                return;
            }

            if (this.MapClickOverride is not null)
            {
                var controller = this.controlWorldMap.WorldMapController;
                var mapPositionAbsolute = controller.PointedMapWorldPositionAbsolute;
                Api.SafeInvoke(() => this.MapClickOverride(mapPositionAbsolute));
                return;
            }

            var character = ClientCurrentCharacterHelper.Character;
            if (character.ProtoCharacter is PlayerCharacterSpectator
                || CreativeModeSystem.SharedIsInCreativeMode(character)
                || Api.IsEditor)
            {
                CallCreativeModeTeleport(worldPosition);
            }
        }

        private void RestoreDefaultVisualizers()
        {
            this.DestroyVisualizers();

            var controller = this.controlWorldMap.WorldMapController;
            if (controller is null)
            {
                throw new InvalidOperationException();
            }

            var landClaimGroupVisualizer = new ClientWorldMapLandClaimsGroupVisualizer(controller);
            this.AddVisualizer(landClaimGroupVisualizer);
            this.AddVisualizer(new ClientWorldMapAllyBaseUnderRaidVisualizer(controller));
            this.AddVisualizer(new ClientWorldMapLandClaimVisualizer(controller, landClaimGroupVisualizer));
            this.AddVisualizer(new ClientWorldMapBedVisualizer(controller));
            this.AddVisualizer(new ClientWorldMapDroppedItemsVisualizer(controller));
            this.AddVisualizer(new ClientWorldMapTradingTerminalsVisualizer(controller));
            this.AddVisualizer(new ClientWorldMapResourcesVisualizer(controller, enableNotifications: true));
            this.AddVisualizer(new ClientWorldMapEventVisualizer(controller));
            this.AddVisualizer(new ClientWorldMapMembersVisualizer(controller));
            this.AddVisualizer(new ClientWorldMapLastVehicleVisualizer(controller));
            this.AddVisualizer(new ClientWorldMapTeleportsVisualizer(controller, isActiveMode: false));
        }

        private void TryActivateWorldMapController()
        {
            if (this.controlWorldMap.WorldMapController is null)
            {
                return;
            }

            switch (this.Window.State)
            {
                case GameWindowState.Opening:
                case GameWindowState.Opened:
                    this.controlWorldMap.WorldMapController.IsActive = true;
                    this.controlWorldMap.WorldMapController.MapClickCallback = this.MapClickHandler;

                    foreach (var visualizer in this.visualizers)
                    {
                        visualizer.IsEnabled = true;
                    }

                    break;
            }
        }
    }
}