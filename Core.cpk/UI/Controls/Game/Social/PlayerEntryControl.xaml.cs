namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class PlayerEntryControl : BaseUserControl
    {
        private static double lastContextMenuCloseFrameTime;

        public BaseCommand CommandInviteToFaction => new ActionCommand(this.ExecuteCommandInviteToFaction);

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
            var viewModel = (ViewModelPlayerEntry)this.DataContext;
            var name = viewModel.Name;
            Api.Client.Core.CopyToClipboard(name);
        }

        private void ExecuteCommandInviteToFaction()
        {
            var viewModel = (ViewModelPlayerEntry)this.DataContext;
            var name = viewModel.Name;
            FactionSystem.ClientOfficerInviteMember(name);
        }

        private void ExecuteCommandInviteToParty()
        {
            var viewModel = (ViewModelPlayerEntry)this.DataContext;
            var name = viewModel.Name;
            PartySystem.ClientInviteMember(name);
        }

        private void ExecuteCommandOpenPrivateChat()
        {
            var viewModel = (ViewModelPlayerEntry)this.DataContext;
            var name = viewModel.Name;
            ChatSystem.ClientOpenPrivateChat(withCharacterName: name);
        }

        private void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            this.DestroyContextMenu();
        }

        private void MouseUpHandler(object sender, MouseEventArgs e)
        {
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

            var viewModel = (ViewModelPlayerEntry)this.DataContext;
            var characterName = viewModel.Name;

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

            if (PartySystem.ClientCanInvite(characterName))
            {
                contextMenuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Chat_MessageMenu_InviteToParty,
                        Command = new ActionCommand(this.ExecuteCommandInviteToParty)
                    });
            }

            if (FactionSystem.ClientCanInviteToFaction(characterName))
            {
                contextMenuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Faction_InviteToFaction,
                        Command = new ActionCommand(this.ExecuteCommandInviteToFaction)
                    });
            }

            contextMenuItems.Add(
                new MenuItem()
                {
                    Header = viewModel.IsBlocked
                                 ? CoreStrings.Chat_MessageMenu_Unblock
                                 : CoreStrings.Chat_MessageMenu_Block,
                    Command = viewModel.CommandToggleBlock
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