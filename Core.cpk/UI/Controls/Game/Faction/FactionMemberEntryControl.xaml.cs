namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionMemberEntryControl : BaseUserControl
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
            var viewModel = (FactionMemberViewEntry)this.DataContext;
            var memberName = viewModel.Name;
            Api.Client.Core.CopyToClipboard(memberName);
        }

        private void ExecuteCommandOpenPrivateChat()
        {
            var viewModel = (FactionMemberViewEntry)this.DataContext;
            var memberName = viewModel.Name;
            ChatSystem.ClientOpenPrivateChat(withCharacterName: memberName);
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

            var viewModel = (FactionMemberViewEntry)this.DataContext;
            var memberName = viewModel.Name;

            if (viewModel.IsCurrentPlayerEntry)
            {
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

            if (FactionSystem.ClientHasAccessRight(FactionMemberAccessRights.SetMemberRole)
                && memberName != ClientCurrentCharacterHelper.Character.Name
                && memberName != FactionSystem.ClientCurrentFactionLeaderName)
            {
                contextMenuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Faction_SetRole,
                        Command = new ActionCommand(
                            () => WindowSetMemberRole.Open(memberName))
                    });
            }

            if (FactionSystem.ClientHasAccessRight(FactionMemberAccessRights.RemoveMembers)
                && memberName != ClientCurrentCharacterHelper.Character.Name
                && memberName != FactionSystem.ClientCurrentFactionLeaderName)
            {
                contextMenuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Faction_RemoveMember,
                        Command = new ActionCommand(
                            () => FactionSystem.ClientOfficerRemoveFactionMember(memberName))
                    });
            }

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