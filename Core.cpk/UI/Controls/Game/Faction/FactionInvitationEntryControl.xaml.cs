namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionInvitationEntryControl : BaseUserControl
    {
        private static double lastContextMenuCloseFrameTime;

        protected override void OnLoaded()
        {
            this.MouseUp += this.MouseUpHandler;
            this.MouseLeave += this.MouseLeaveHandler;
        }

        protected override void OnUnloaded()
        {
            this.MouseUp -= this.MouseUpHandler;
            this.MouseLeave -= this.MouseLeaveHandler;
        }

        private void ContextMenuClosedHandler(object sender, RoutedEventArgs e)
        {
            var contextMenu = (ContextMenu)sender;
            contextMenu.Closed -= this.ContextMenuClosedHandler;
            this.ContextMenu = null;
            lastContextMenuCloseFrameTime = Api.Client.Core.ClientRealTime;
        }

        private void DestroyContextMenu()
        {
            var m = this.ContextMenu;
            if (m is null)
            {
                return;
            }

            m.IsOpen = false;
            this.ContextMenu = null;
        }

        private void ExecuteCommandCopyName()
        {
            var viewModel = (FactionInvitationOfficerViewEntry)this.DataContext;
            var inviteeName = viewModel.InviteeName;
            Api.Client.Core.CopyToClipboard(inviteeName);
        }

        private void ExecuteCommandOpenPrivateChat()
        {
            var viewModel = (FactionInvitationOfficerViewEntry)this.DataContext;
            var inviteeName = viewModel.InviteeName;
            ChatSystem.ClientOpenPrivateChat(withCharacterName: inviteeName);
        }

        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            this.DestroyContextMenu();
        }

        private void MouseUpHandler(object sender, MouseEventArgs e)
        {
            if (!this.IsHitTestVisible)
            {
                return;
            }

            var contextMenu = this.ContextMenu;
            if (contextMenu is not null
                && contextMenu.IsOpen)
            {
                // close current context menu
                contextMenu.IsOpen = false;
                this.ContextMenu = null;
                return;
            }

            if (lastContextMenuCloseFrameTime + 0.2 >= Api.Client.Core.ClientRealTime)
            {
                // just closed a context menu
                return;
            }

            // create new context menu
            contextMenu = new ContextMenu();
            var contextMenuItems = contextMenu.Items;

            contextMenuItems.Add(
                new MenuItem()
                {
                    Header = CoreStrings.Chat_MessageMenu_CopyName,
                    Command = new ActionCommand(this.ExecuteCommandCopyName)
                });

            contextMenuItems.Add(
                new MenuItem()
                {
                    Header = CoreStrings.Chat_MessageMenu_PrivateMessage,
                    Command = new ActionCommand(this.ExecuteCommandOpenPrivateChat)
                });

            this.ContextMenu = contextMenu;

            contextMenu.Placement = PlacementMode.Relative;
            var target = sender as UIElement;
            contextMenu.PlacementTarget = target;
            var position = e.GetPosition(target);
            contextMenu.HorizontalOffset = position.X;
            contextMenu.VerticalOffset = position.Y;
            contextMenu.IsOpen = true;
            contextMenu.Closed += this.ContextMenuClosedHandler;
        }
    }
}