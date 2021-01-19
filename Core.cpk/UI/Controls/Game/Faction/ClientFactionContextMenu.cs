namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public static class ClientFactionContextMenu
    {
        public static void Show(
            string clanTag,
            bool addShowFactionInformationMenuEntry)
        {
            var isCurrentClientFaction = clanTag == FactionSystem.ClientCurrentFactionClanTag;

            // create new context menu
            var menuItems = new List<MenuItem>();
            menuItems.Add(
                new MenuItem()
                {
                    Header = CoreStrings.Faction_CopyTag,
                    Command = new ActionCommand(
                        () => Api.Client.Core.CopyToClipboard($"[{clanTag}]"))
                });

            if (addShowFactionInformationMenuEntry)
            {
                menuItems.Add(
                    new MenuItem()
                    {
                        Header = CoreStrings.Faction_ShowFactionInformation,
                        Command = new ActionCommand(
                            () => FactionDetailsControl.Show(clanTag))
                    });
            }

            menuItems.Add(
                new MenuItem()
                {
                    Header = CoreStrings.Faction_PrivateMessageToLeader,
                    Command = new ActionCommand(
                        () => FactionSystem.ClientOpenPrivateChatWithFactionLeader(clanTag))
                });

            if (FactionSystem.ClientHasFaction
                && !isCurrentClientFaction
                && FactionSystem.SharedIsDiplomacyFeatureAvailable
                && FactionSystem.ClientHasAccessRight(FactionMemberAccessRights.DiplomacyManagement))
            {
                AddDiplomacyManagementMenuEntries(clanTag, menuItems);
            }

            ClientContextMenuHelper.ShowMenuOnClick("FactionContextMenu", menuItems);
        }

        private static void AddDiplomacyManagementMenuEntries(string clanTag, List<MenuItem> menuItems)
        {
            var factionPrivateState = Faction.GetPrivateState(FactionSystem.ClientCurrentFaction);
            var status = FactionSystem.ClientGetCurrentFactionDiplomacyStatus(clanTag);
            if (status != FactionDiplomacyStatus.Ally
                && FactionConstants.SharedPvpAlliancesEnabled)
            {
                // ally proposal management
                var hasIncomingAllianceRequest = factionPrivateState.IncomingFactionAllianceRequests
                                                                    .TryGetValue(clanTag, out var incomingRequest)
                                                 && !incomingRequest.IsRejected;

                var hasOutgoingAllianceRequest = factionPrivateState.OutgoingFactionAllianceRequests
                                                                    .TryGetValue(clanTag, out var outgoingRequest)
                                                 && !outgoingRequest.IsRejected;

                if (hasIncomingAllianceRequest)
                {
                    menuItems.Add(
                        new MenuItem()
                        {
                            Header = CoreStrings.Faction_Diplomacy_AllianceRequest_AcceptIncoming,
                            Command = new ActionCommand(
                                () => FactionSystem.ClientOfficerAcceptAlliance(clanTag))
                        });

                    menuItems.Add(
                        new MenuItem()
                        {
                            Header = CoreStrings.Faction_Diplomacy_AllianceRequest_RejectIncoming,
                            Command = new ActionCommand(
                                () => FactionSystem.ClientOfficerRejectAlliance(clanTag))
                        });
                }

                if (hasOutgoingAllianceRequest)
                {
                    menuItems.Add(
                        new MenuItem()
                        {
                            Header = CoreStrings.Faction_Diplomacy_AllianceRequest_CancelOutgoing,
                            Command = new ActionCommand(
                                () => FactionSystem.ClientOfficerCancelOutgoingAllianceRequest(clanTag))
                        });
                }
                else if (!hasIncomingAllianceRequest)
                {
                    menuItems.Add(
                        new MenuItem()
                        {
                            Header = CoreStrings.Faction_Diplomacy_ProposeAlliance,
                            Command = new ActionCommand(
                                () => FactionSystem.ClientOfficerProposeAlliance(clanTag))
                        });
                }
            }

            switch (status)
            {
                case FactionDiplomacyStatus.Ally:
                    menuItems.Add(
                        new MenuItem()
                        {
                            Header = CoreStrings.Faction_Diplomacy_BreakAlliance,
                            Command = new ActionCommand(
                                () => DialogWindow.ShowDialog(
                                    title: CoreStrings.QuestionAreYouSure,
                                    text: CoreStrings.Faction_Diplomacy_BreakAlliance,
                                    okText: CoreStrings.Yes,
                                    okAction: () => FactionSystem.ClientOfficerSetDiplomacyStatus(
                                                  clanTag,
                                                  FactionSystem.FactionDiplomacyStatusChangeRequest.Neutral),
                                    cancelText: CoreStrings.Button_Cancel,
                                    cancelAction: () => { },
                                    focusOnCancelButton: true))
                        });
                    break;

                case FactionDiplomacyStatus.Neutral:
                case FactionDiplomacyStatus.EnemyDeclaredByOtherFaction:
                    menuItems.Add(
                        new MenuItem()
                        {
                            Header = CoreStrings.Faction_Diplomacy_DeclareWar,
                            Command = new ActionCommand(
                                () => DialogWindow.ShowDialog(
                                    title: CoreStrings.QuestionAreYouSure,
                                    text: CoreStrings.Faction_Diplomacy_DeclareWar,
                                    okText: CoreStrings.Yes,
                                    okAction: () => FactionSystem.ClientOfficerSetDiplomacyStatus(
                                                  clanTag,
                                                  FactionSystem.FactionDiplomacyStatusChangeRequest
                                                               .Enemy),
                                    cancelText: CoreStrings.Button_Cancel,
                                    cancelAction: () => { },
                                    focusOnCancelButton: true))
                        });
                    break;

                case FactionDiplomacyStatus.EnemyMutual:
                case FactionDiplomacyStatus.EnemyDeclaredByCurrentFaction:
                    menuItems.Add(
                        new MenuItem()
                        {
                            Header = CoreStrings.Faction_Diplomacy_WithdrawWarDeclaration,
                            Command = new ActionCommand(
                                () => DialogWindow.ShowDialog(
                                    title: CoreStrings.QuestionAreYouSure,
                                    text: CoreStrings.Faction_Diplomacy_WithdrawWarDeclaration,
                                    okText: CoreStrings.Yes,
                                    okAction: () => FactionSystem.ClientOfficerSetDiplomacyStatus(
                                                  clanTag,
                                                  FactionSystem.FactionDiplomacyStatusChangeRequest
                                                               .Neutral),
                                    cancelText: CoreStrings.Button_Cancel,
                                    cancelAction: () => { },
                                    focusOnCancelButton: true))
                        });
                    break;
            }
        }
    }
}