namespace AtomicTorch.CBND.CoreMod.Systems.Party
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using AtomicTorch.CBND.CoreMod.Systems.Chat;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Notifications;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ClientPartyInvitationNotificationSystem
        : ProtoSystem<ClientPartyInvitationNotificationSystem>
    {
        public const string InvitationMessageFormat
            = "[b]{0}[/b] invited you to join their party.";

        public const string InvitationMessageYouWillLeaveYourParty
            = "Please note: you will [b]leave[/b] your current party if you accept this invitation.";

        public const string InvitationSentMessageFormat
            = "Party invite to [b]{0}[/b] has been sent.";

        public const string PartyInvitationTitle
            = "Party invitation";

        private static readonly TextureResource IconPartyInvitation
            = new TextureResource("Icons/IconPartyInvitation");

        private static readonly Dictionary<string, WeakReference<HudNotificationControl>>
            NotificationsFromInviteeDictionary
                = new Dictionary<string, WeakReference<HudNotificationControl>>(StringComparer.Ordinal);

        public override string Name => "Party invitations (client) system";

        public static void ShowNotificationInviteSent(string name)
        {
            NotificationSystem.ClientShowNotification(
                title: PartyInvitationTitle,
                message: string.Format(InvitationSentMessageFormat, name),
                autoHide: true,
                icon: IconPartyInvitation);
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                PartySystem.ClientCurrentInvitationsFromCharacters.CollectionChanged
                    += InvitationsCollectionChangedHandler;
            }
        }

        private static void InvitationsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            string inviterName;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    inviterName = (string)e.NewItems[0];
                    ShowNotification(inviterName);
                    break;
                }

                case NotifyCollectionChangedAction.Remove:
                {
                    inviterName = (string)e.OldItems[0];
                    if (NotificationsFromInviteeDictionary.TryGetValue(inviterName, out var weakReference)
                        && weakReference.TryGetTarget(out var control))
                    {
                        control.Hide(quick: true);
                    }

                    NotificationsFromInviteeDictionary.Remove(inviterName);
                    break;
                }

                case NotifyCollectionChangedAction.Reset:
                {
                    // hide all current notifications
                    foreach (var notificationControl in NotificationsFromInviteeDictionary)
                    {
                        var weakReference = notificationControl.Value;
                        if (weakReference.TryGetTarget(out var control))
                        {
                            control.Hide(quick: true);
                        }
                    }

                    NotificationsFromInviteeDictionary.Clear();

                    // display new notifications
                    foreach (var name in PartySystem.ClientCurrentInvitationsFromCharacters)
                    {
                        ShowNotification(name);
                    }

                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            void ShowNotification(string name)
            {
                if (ClientChatBlockList.IsBlocked(name))
                {
                    // don't display invitations from blocked players
                    return;
                }

                var control = NotificationSystem.ClientShowNotification(
                    title: PartyInvitationTitle,
                    message: string.Format(InvitationMessageFormat, name),
                    onClick: () => ShowInvitationDialog(name),
                    autoHide: false,
                    icon: IconPartyInvitation);

                control.CallbackOnRightClickHide = () => PartySystem.ClientInvitationDecline(name);

                NotificationsFromInviteeDictionary.Add(
                    name,
                    new WeakReference<HudNotificationControl>(control));
            }
        }

        private static void ShowInvitationDialog(string inviterName)
        {
            var text = string.Format(InvitationMessageFormat, inviterName);
            if (PartySystem.ClientGetCurrentPartyMembers().Count > 1)
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                text += "[br]" + InvitationMessageYouWillLeaveYourParty;
            }

            DialogWindow.ShowDialog(
                title: PartyInvitationTitle,
                text: text,
                okText: CoreStrings.Button_Accept,
                okAction: () => PartySystem.ClientInvitationAccept(inviterName),
                cancelText: CoreStrings.Button_Deny,
                cancelAction: () => PartySystem.ClientInvitationDecline(inviterName),
                focusOnCancelButton: true,
                closeByEscapeKey: false);
        }
    }
}