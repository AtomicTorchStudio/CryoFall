namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientContextMenuHelper
    {
        private static ContextMenu lastContextMenu;

        private static double lastContextMenuCloseFrameTime;

        public static void CloseLastContextMenuFor(object key)
        {
            if ((lastContextMenu?.IsOpen ?? false)
                && Equals(lastContextMenu.Tag, key))
            {
                lastContextMenu.IsOpen = false;
            }
        }

        public static void ShowMenuOnClick(object key, List<MenuItem> menuItems)
        {
            if (lastContextMenu?.IsOpen ?? false)
            {
                // close the last menu, don't open the new menu until requested again
                lastContextMenu.IsOpen = false;
                return;
            }

            if (lastContextMenuCloseFrameTime + 0.2 >= Api.Client.Core.ClientRealTime)
            {
                // just closed a context menu
                return;
            }

            if (menuItems.Count == 0)
            {
                return;
            }

            var contextMenu = new ContextMenu();
            var contextMenuItems = contextMenu.Items;
            foreach (var menuItem in menuItems)
            {
                contextMenuItems.Add(menuItem);
            }

            var visual = Api.Client.UI.GetVisualInPointedPosition();
            var target = visual switch
            {
#if GAME
                System.Windows.Documents.Run run =>  LogicalTreeHelper.GetParent(visual) as FrameworkElement,
#endif
                FrameworkElement frameworkElement => frameworkElement,
                null                              => null,
                _                                 => LogicalTreeHelper.GetParent(visual) as FrameworkElement
            };

            if (target is null)
            {
                Api.Logger.Error("Cannot open a context menu here - no control under the mouse button");
                return;
            }

            target.ContextMenu = contextMenu;
            contextMenu.Placement = PlacementMode.Relative;
            contextMenu.PlacementTarget = target;
            var position = target.PointFromScreen(
                new Point(Api.Client.Input.MouseScreenPosition.X,
                          Api.Client.Input.MouseScreenPosition.Y));

            contextMenu.HorizontalOffset = position.X;
            contextMenu.VerticalOffset = position.Y;
            contextMenu.IsOpen = true;
            contextMenu.Tag = key;
            contextMenu.Closed += ContextMenuClosedHandler;
            lastContextMenu = contextMenu;
        }

        private static void ContextMenuClosedHandler(object sender, RoutedEventArgs e)
        {
            var contextMenu = (ContextMenu)sender;
            contextMenu.Closed -= ContextMenuClosedHandler;
            contextMenu.Tag = null;

            if (contextMenu.PlacementTarget is FrameworkElement owner)
            {
                owner.ContextMenu = null;
            }

            contextMenu.PlacementTarget = null;
            if (ReferenceEquals(lastContextMenu, contextMenu))
            {
                lastContextMenu = null;
            }

            lastContextMenuCloseFrameTime = Api.Client.Core.ClientRealTime;
        }
    }
}