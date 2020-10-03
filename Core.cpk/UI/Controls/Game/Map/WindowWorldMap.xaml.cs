namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ConsoleCommands.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.ServerOperator;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public partial class WindowWorldMap : BaseWindowMenu
    {
        public const string ContextMenuCopyCoordinates = "Copy coordinates";

        public const string ContextMenuTeleport = "Teleport";

        private ControlWorldMap controlWorldMap;

        private IWorldMapVisualizer[] visualisers;

        protected override void InitMenu()
        {
            base.InitMenu();
            this.controlWorldMap = this.GetByName<ControlWorldMap>("ControlWorldMap");
        }

        protected override void OnLoaded()
        {
            var controller = this.controlWorldMap.WorldMapController;

            var landClaimGroupVisualizer = new ClientWorldMapLandClaimsGroupVisualizer(controller);
            this.visualisers = new IWorldMapVisualizer[]
            {
                landClaimGroupVisualizer,
                new ClientWorldMapLandClaimVisualizer(controller, landClaimGroupVisualizer),
                new ClientWorldMapBedVisualizer(controller),
                new ClientWorldMapDroppedItemsVisualizer(controller),
                new ClientWorldMapTradingTerminalsVisualizer(controller),
                new ClientWorldMapResourcesVisualizer(controller, enableNotifications: true),
                new ClientWorldMapEventVisualizer(controller, enableNotifications: true),
                new ClientWorldMapPartyMembersVisualizer(controller),
                new ClientWorldMapLastVehicleVisualizer(controller)
            };
        }

        protected override void OnUnloaded()
        {
            foreach (var visualiser in this.visualisers)
            {
                try
                {
                    visualiser.Dispose();
                }
                catch (Exception ex)
                {
                    Api.Logger.Exception(ex, "Exception during visualizer disposing");
                }
            }

            this.visualisers = Array.Empty<IWorldMapVisualizer>();
        }

        protected override void WindowClosed()
        {
            this.controlWorldMap.WorldMapController.IsActive = false;
            foreach (var visualiser in this.visualisers)
            {
                visualiser.IsEnabled = false;
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
            this.controlWorldMap.WorldMapController.IsActive = true;
            this.controlWorldMap.WorldMapController.MapClickCallback = this.MapClickHandler;
            this.controlWorldMap.MouseRightButtonUp += this.ControlWorldMapMouseRightButtonUpHandler;

            foreach (var visualiser in this.visualisers)
            {
                visualiser.IsEnabled = true;
            }

            base.WindowOpening();
        }

        private async void CallTeleport(Vector2D worldPosition)
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

        private void ControlWorldMapMouseRightButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            var mapPositionWithOffset = this.controlWorldMap.WorldMapController.PointedMapPositionWithOffset;
            this.CloseContextMenu();

            var contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem()
            {
                Header = ContextMenuCopyCoordinates,
                Command = new ActionCommand(
                    () => Api.Client.Core.CopyToClipboard(mapPositionWithOffset.ToString()))
            });

            var character = Api.Client.Characters.CurrentPlayerCharacter;
            if (character.ProtoCharacter is PlayerCharacterSpectator
                || ServerOperatorSystem.SharedIsOperator(character)
                || CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                var mapPositionWithoutOffset = this.controlWorldMap.WorldMapController.PointedMapPositionWithoutOffset;
                contextMenu.Items.Add(new MenuItem()
                {
                    Header = ContextMenuTeleport,
                    Command = new ActionCommand(
                        () => this.CallTeleport(mapPositionWithoutOffset.ToVector2D()))
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

        private void MapClickHandler(Vector2D worldPosition)
        {
            if (this.ContextMenu is not null)
            {
                // context menu is still exist, don't process this click
                return;
            }

            var character = ClientCurrentCharacterHelper.Character;
            if (character.ProtoCharacter is PlayerCharacterSpectator
                || CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                this.CallTeleport(worldPosition);
            }
        }
    }
}