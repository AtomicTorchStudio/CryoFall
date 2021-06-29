namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data
{
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWindowFaction : BaseViewModel
    {
        private readonly FactionPrivateState factionPrivateState;

        private readonly FactionPublicState factionPublicState;

        public ViewModelWindowFaction()
        {
            var faction = FactionSystem.ClientCurrentFaction;
            this.factionPublicState = Faction.GetPublicState(faction);
            this.factionPrivateState = Faction.GetPrivateState(faction);

            FactionSystem.ClientCurrentFactionMemberAddedOrRemoved
                += this.ClientCurrentFactionMemberAddedOrRemovedHandler;

            FactionConstants.ClientFactionMembersMaxChanged
                += this.FactionMembersMaxNumberChangedHandler;

            FactionConstants.ClientSharedFactionUpgradeCostsChanged
                += this.FactionUpgradeCostsChangedHandler;

            FactionSystem.ClientCurrentFactionReceivedApplications.CollectionChanged
                += this.ReceivedApplicationsCollectionChangedHandler;

            this.factionPrivateState.IncomingFactionAllianceRequests.ClientAnyModification
                += this.IncomingFactionAllianceRequestsOnClientAnyModification;

            this.factionPrivateState.AccessRightsBinding.ClientAnyModification
                += this.AccessRightsAnyModificationHandler;

            ClientComponentTechnologiesWatcher.LearningPointsChanged
                += this.LearningPointsChangedHandler;

            ClientLandClaimAreaManager.AreaAdded += this.LandClaimAreaAddedOrRemovedHandler;
            ClientLandClaimAreaManager.AreaRemoved += this.LandClaimAreaAddedOrRemovedHandler;

            this.factionPublicState.ClientSubscribe(
                _ => _.ClanTag,
                () => this.NotifyPropertyChanged(nameof(this.ClanTag)),
                subscriptionOwner: this);

            this.factionPublicState.ClientSubscribe(
                _ => _.Emblem,
                () => this.NotifyPropertyChanged(nameof(this.Emblem)),
                subscriptionOwner: this);

            this.factionPublicState.ClientSubscribe(
                _ => _.LeaderName,
                () => this.NotifyPropertyChanged(nameof(this.LeaderName)),
                subscriptionOwner: this);

            this.factionPublicState.ClientSubscribe(
                _ => _.PlayersNumberCurrent,
                () => this.NotifyPropertyChanged(nameof(this.MembersNumberCurrent)),
                subscriptionOwner: this);

            this.factionPublicState.ClientSubscribe(
                _ => _.PlayersNumberCurrent,
                () => this.NotifyPropertyChanged(nameof(this.MembersNumberText)),
                subscriptionOwner: this);

            this.factionPublicState.ClientSubscribe(
                _ => _.Level,
                () =>
                {
                    this.NotifyPropertyChanged(nameof(this.FactionLevel));
                    this.NotifyPropertyChanged(nameof(this.IsFactionMaxLevelReached));
                    this.NotifyPropertyChanged(nameof(this.FactionLevelUpgradeCostLearningPoints));
                    this.NotifyPropertyChanged(nameof(this.FactionLevelUpgradeNextLevelInfoText));
                    this.NotifyPropertyChanged(nameof(this.CanUpgradeFactionLevel));
                },
                subscriptionOwner: this);

            this.factionPublicState.ClientSubscribe(
                _ => _.IsAcceptingApplications,
                () => this.NotifyPropertyChanged(nameof(this.IsAcceptingApplications)),
                subscriptionOwner: this);

            this.factionPublicState.ClientSubscribe(
                _ => _.TotalScore,
                () =>
                {
                    this.NotifyPropertyChanged(nameof(this.TotalScore));
                    this.NotifyPropertyChanged(nameof(this.TotalScoreString));
                },
                subscriptionOwner: this);

            this.factionPublicState.ClientSubscribe(
                _ => _.LeaderboardRank,
                () => this.NotifyPropertyChanged(nameof(this.LeaderboardRank)),
                subscriptionOwner: this);

            this.factionPrivateState.ClientSubscribe(
                _ => _.DescriptionPrivate,
                () => this.NotifyPropertyChanged(nameof(this.DescriptionPrivate)),
                subscriptionOwner: this);

            this.factionPrivateState.ClientSubscribe(
                _ => _.DescriptionPublic,
                () => this.NotifyPropertyChanged(nameof(this.DescriptionPublic)),
                subscriptionOwner: this);

            this.factionPrivateState.ClientSubscribe(
                _ => _.AccumulatedLearningPointsForUpgrade,
                () => this.NotifyPropertyChanged(nameof(this.FactionAccumulatedLearningPointsForUpgrade)),
                subscriptionOwner: this);

            this.ViewModelFactionAdmin = new ViewModelFactionAdmin(this.factionPrivateState);

            this.Refresh();

            ClientTimersSystem.AddAction(2,
                                         () =>
                                         {
                                             if (!this.IsDisposed)
                                             {
                                                 // a workaround for the notification icon in HUDButtonsBar
                                                 this.NotifyPropertyChanged(nameof(this.ReceivedApplicationsCount));
                                             }
                                         });
        }

        public bool CanUpgradeFactionLevel
            => ClientCurrentCharacterHelper.PrivateState.Technologies.LearningPoints
               >= 1;

        public string ClanTag => this.factionPublicState?.ClanTag;

        public BaseCommand CommandCancelAllInvitations
            => new ActionCommand(this.ExecuteCommandCancelAllInvitations);

        public BaseCommand CommandInvite
            => new ActionCommand(this.ExecuteCommandInvite);

        public BaseCommand CommandOpenDissolveFactionDialog
            => new ActionCommand(ExecuteCommandOpenDissolveFactionDialog);

        public BaseCommand CommandOpenEmblemEditor
            => new ActionCommand(this.ExecuteCommandOpenEmblemEditor);

        public BaseCommand CommandOpenLeaveFactionDialog
            => new ActionCommand(this.ExecuteCommandOpenLeaveFactionDialog);

        public BaseCommand CommandOpenTransferOwnershipDialog { get; }
            = new ActionCommand(ExecuteCommandOpenTransferOwnershipDialog);

        public BaseCommand CommandRejectAllApplications
            => new ActionCommand(this.ExecuteCommandRejectAllApplications);

        public BaseCommand CommandUpgradeFactionLevel
            => new ActionCommand(WindowFactionLearningPointsDonation.Open);

        public bool CurrentPlayerIsLeader
            => FactionSystem.ClientCurrentRole == FactionMemberRole.Leader;

        public string CurrentPlayerRolePermissionsText
        {
            get
            {
                var accessRights = FactionSystem.SharedGetRoleAccessRights(
                    FactionSystem.ClientCurrentFaction,
                    FactionSystem.ClientCurrentRole);
                if (accessRights == FactionMemberAccessRights.None)
                {
                    return CoreStrings.Faction_CurrentRole_Permissions_None;
                }

                var sb = new StringBuilder();
                var isFirst = true;
                foreach (var accessRight in EnumExtensions.GetValues<FactionMemberAccessRights>())
                {
                    if (accessRight == FactionMemberAccessRights.None
                        || accessRight == FactionMemberAccessRights.Leader)
                    {
                        continue;
                    }

                    if (!accessRights.HasFlag(accessRight))
                    {
                        continue;
                    }

                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        sb.AppendLine("[br]");
                    }

                    sb.Append("[*]")
                      .Append(accessRight.GetDescription());

                    if (accessRight.GetAttribute<DescriptionTooltipAttribute>() is { } descriptionTooltip)
                    {
                        sb.Append("[br]")
                          .Append(descriptionTooltip.TooltipMessage);
                    }
                }

                return sb.ToString();
            }
        }

        public string CurrentPlayerRoleTitle
            => FactionSystem.ClientGetRoleTitle(
                FactionSystem.ClientCurrentRole);

        public string DescriptionPrivate
        {
            get
            {
                var text = this.factionPrivateState.DescriptionPrivate;
                return string.IsNullOrEmpty(text) ? CoreStrings.EmptyText : text;
            }
        }

        public string DescriptionPublic
        {
            get
            {
                var text = this.factionPrivateState.DescriptionPublic;
                return string.IsNullOrEmpty(text) ? CoreStrings.EmptyText : text;
            }
        }

        public Brush Emblem
            => Client.UI.GetTextureBrush(
                ClientFactionEmblemTextureProvider.GetEmblemTexture(
                    this.factionPublicState.Emblem,
                    useCache: true));

        public ushort FactionAccumulatedLearningPointsForUpgrade
            => this.factionPrivateState.AccumulatedLearningPointsForUpgrade;

        public string FactionKindDescription
            => this.factionPublicState.Kind.GetAttribute<DescriptionTooltipAttribute>()
                   .TooltipMessage;

        public string FactionKindTitle
            => this.factionPublicState.Kind.GetDescription();

        public string FactionLandClaimsNumberText
        {
            get
            {
                var current = LandClaimSystem.ClientEnumerateAllCurrentFactionAreas()
                                             .Count();
                var max = FactionConstants.SharedGetFactionLandClaimsLimit(this.FactionLevel);
                var message = string.Format(CoreStrings.Faction_LandClaimNumberLimit_Format,
                                            current,
                                            max);

                if (current == max)
                {
                    return message;
                }

                if (this.FactionLevel + 1 < FactionConstants.MaxFactionLevel)
                {
                    message += "[br]";
                    message += CoreStrings.Faction_LandClaimNumberLimit_CanIncrease;
                }

                return message;
            }
        }

        public byte FactionLevel => this.factionPublicState.Level;

        public ushort FactionLevelUpgradeCostLearningPoints
            => this.IsFactionMaxLevelReached
                   ? (ushort)0
                   : FactionConstants.SharedGetFactionUpgradeCost((byte)(this.FactionLevel + 1));

        public string FactionLevelUpgradeNextLevelInfoText
        {
            get
            {
                if (this.IsFactionMaxLevelReached)
                {
                    return string.Empty;
                }

                var currentLimit = FactionConstants.SharedGetFactionLandClaimsLimit(this.FactionLevel);
                var nextLimit = FactionConstants.SharedGetFactionLandClaimsLimit((byte)(this.FactionLevel + 1));
                var deltaLimit = nextLimit - currentLimit;

                if (deltaLimit == 0)
                {
                    // the next level will not raise the land claims number limit
                    if (this.FactionLevel + 1 < FactionConstants.MaxFactionLevel)
                    {
                        // probably higher level will allow to raise the limit
                        return CoreStrings.Faction_LandClaimNumberLimit_CanIncrease;
                    }

                    // max level reached
                    return this.FactionLandClaimsNumberText;
                }

                return string.Format(CoreStrings.Faction_LandClaimNumberLimit_Format
                                                .Replace("/",   string.Empty)
                                                .Replace("{1}", string.Empty),
                                     "[b]+" + deltaLimit + "[/b]");
            }
        }

        public bool HasOfficerAccessRightDiplomacyManagement
            => FactionSystem.ClientHasAccessRight(FactionMemberAccessRights.DiplomacyManagement);

        public bool HasOfficerAccessRightEditDescription
            => FactionSystem.ClientHasAccessRight(FactionMemberAccessRights.EditDescription);

        public bool HasOfficerAccessRightRecruitment
            => FactionSystem.ClientHasAccessRight(FactionMemberAccessRights.Recruitment);

        /// <summary>
        /// Only officer will see the number of active requests in the tab badge.
        /// </summary>
        public int IncomingAllianceRequestsCount
            => this.HasOfficerAccessRightDiplomacyManagement
                   ? this.factionPrivateState.IncomingFactionAllianceRequests
                         .Count(e => !e.Value.IsRejected)
                   : 0;

        public string InviteeName { get; set; }

        public bool IsAcceptingApplications
        {
            get => this.factionPublicState.IsAcceptingApplications;
            set
            {
                if (this.factionPublicState.IsAcceptingApplications == value)
                {
                    return;
                }

                this.factionPublicState.IsAcceptingApplications = value;
                FactionSystem.ClientOfficerSetIsAcceptingApplications(value);
                this.NotifyThisPropertyChanged();
            }
        }

        public bool IsDiplomacyTabAvailable
            => FactionSystem.SharedIsDiplomacyFeatureAvailable;

        public bool IsFactionMaxLevelReached => this.FactionLevel >= FactionConstants.MaxFactionLevel;

        public bool IsPrivateFaction
            => this.factionPublicState.Kind == FactionKind.Private;

        public bool IsPublicFaction => this.factionPublicState.Kind == FactionKind.Public;

        public ushort LeaderboardRank => this.factionPublicState.LeaderboardRank;

        public string LeaderName
            => this.factionPublicState.LeaderName;

        public ushort MembersNumberCurrent => this.factionPublicState.PlayersNumberCurrent;

        public int MembersNumberMax
            => FactionConstants.GetFactionMembersMax(this.factionPublicState.Kind);

        public string MembersNumberText
            => string.Format(CoreStrings.Faction_MembersNumber_Format,
                             this.MembersNumberCurrent,
                             this.MembersNumberMax);

        public int ReceivedApplicationsCount => FactionSystem.ClientCurrentFactionReceivedApplications.Count;

        public ulong TotalScore => this.factionPublicState.TotalScore;

        public string TotalScoreString => this.factionPublicState.TotalScore.ToString("#,##0");

        public ViewModelFactionAdmin ViewModelFactionAdmin { get; }

        protected override void DisposeViewModel()
        {
            FactionSystem.ClientCurrentFactionMemberAddedOrRemoved
                -= this.ClientCurrentFactionMemberAddedOrRemovedHandler;

            FactionConstants.ClientFactionMembersMaxChanged
                -= this.FactionMembersMaxNumberChangedHandler;

            FactionConstants.ClientSharedFactionUpgradeCostsChanged
                -= this.FactionUpgradeCostsChangedHandler;

            FactionSystem.ClientCurrentFactionReceivedApplications.CollectionChanged
                -= this.ReceivedApplicationsCollectionChangedHandler;

            this.factionPrivateState.IncomingFactionAllianceRequests.ClientAnyModification
                -= this.IncomingFactionAllianceRequestsOnClientAnyModification;

            this.factionPrivateState.AccessRightsBinding.ClientAnyModification
                -= this.AccessRightsAnyModificationHandler;

            ClientComponentTechnologiesWatcher.LearningPointsChanged
                -= this.LearningPointsChangedHandler;

            ClientLandClaimAreaManager.AreaAdded -= this.LandClaimAreaAddedOrRemovedHandler;
            ClientLandClaimAreaManager.AreaRemoved -= this.LandClaimAreaAddedOrRemovedHandler;

            base.DisposeViewModel();
        }

        private static void ExecuteCommandOpenDissolveFactionDialog()
        {
            var brushColorRed = Api.Client.UI.GetApplicationResource<Brush>("BrushColorRed6");
            if (LandClaimSystem.ClientEnumerateAllCurrentFactionAreas()
                               .Any())
            {
                DialogWindow.ShowDialog(
                    title: CoreStrings.Faction_DialogDissolveFaction_Title,
                    new FormattedTextBlock()
                    {
                        Text = CoreStrings.Faction_DialogDissolveFaction_ErrorHasLandClaimsClaims,
                        FontWeight = FontWeights.Bold,
                        Foreground = brushColorRed
                    },
                    okAction: () => { },
                    closeByEscapeKey: true);
                return;
            }

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(
                new FormattedTextBlock()
                {
                    Text = CoreStrings.Faction_DialogDissolveFaction_Message,
                    FontWeight = FontWeights.Bold,
                    Foreground = brushColorRed
                });

            var checkbox1Message = CoreStrings.Faction_DialogDissolveFaction_Checkbox1Format;
            var checkbox1 = new CheckBox()
            {
                Content = new FormattedTextBlock() { Content = checkbox1Message },
                Margin = new Thickness(0, 10, 0, 0)
            };

            stackPanel.Children.Add(checkbox1);

            var checkbox2Message = string.Format(CoreStrings.Faction_DialogDissolveFaction_Checkbox2Format,
                                                 ClientTimeFormatHelper.FormatTimeDuration(
                                                     FactionConstants.SharedFactionJoinCooldownDuration));

            var checkbox2 = new CheckBox()
            {
                Content = new FormattedTextBlock() { Content = checkbox2Message },
                Margin = new Thickness(0, 10, 0, 0)
            };

            stackPanel.Children.Add(checkbox2);

            DialogWindow dialogWindow = null;
            dialogWindow = DialogWindow.ShowDialog(
                title: CoreStrings.Faction_DialogDissolveFaction_Title,
                content: stackPanel,
                okText: CoreStrings.Yes,
                okAction: DialogOkActionHandler,
                cancelText: CoreStrings.Button_Cancel,
                cancelAction: () => { },
                focusOnCancelButton: true,
                closeByEscapeKey: true);
            dialogWindow.AutoCloseOnOk = false;

            var multiBinding = new MultiBinding();
            multiBinding.Converter = new AllBoolMultiConverter();
            multiBinding.Bindings.Add(new Binding()
            {
                Source = checkbox1,
                Path = new PropertyPath(ToggleButton.IsCheckedProperty),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });
            multiBinding.Bindings.Add(new Binding()
            {
                Source = checkbox2,
                Path = new PropertyPath(ToggleButton.IsCheckedProperty),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            dialogWindow.ButtonOk.SetBinding(UIElement.IsEnabledProperty, multiBinding);

            void DialogOkActionHandler()
            {
                if (checkbox1.IsChecked != true
                    || checkbox2.IsChecked != true)
                {
                    return;
                }

                FactionSystem.ClientLeaderDissolveFaction();
                // ReSharper disable once PossibleNullReferenceException
                // ReSharper disable once AccessToModifiedClosure
                dialogWindow.Close(DialogResult.OK);
            }
        }

        private static void ExecuteCommandOpenTransferOwnershipDialog()
        {
            var stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Vertical;
            stackPanel.Children.Add(
                new FormattedTextBlock()
                {
                    Content = CoreStrings.Faction_DialogOwnershipTransfer_Message,
                    FontWeight = FontWeights.Bold,
                    Foreground = Api.Client.UI.GetApplicationResource<Brush>("BrushColorRed6")
                });

            var textBoxMemberName = new TextBox();
            textBoxMemberName.Margin = new Thickness(0, 5, 0, 5);
            stackPanel.Children.Add(textBoxMemberName);

            DialogWindow dialogWindow = null;
            dialogWindow = DialogWindow.ShowDialog(CoreStrings.Faction_DialogOwnershipTransfer_Title,
                                                   stackPanel,
                                                   okAction: DialogOkHandler,
                                                   okText: CoreStrings.Button_Apply,
                                                   cancelAction: () => { });
            dialogWindow.Window.FocusOnControl = textBoxMemberName;

            void DialogOkHandler()
            {
                var memberName = textBoxMemberName.Text?.Trim();
                if (string.IsNullOrEmpty(memberName))
                {
                    return;
                }

                if (!FactionSystem.ClientIsFactionMember(memberName))
                {
                    NotificationSystem.ClientShowNotification(
                        null,
                        CoreStrings.PlayerNotFound,
                        NotificationColor.Bad);
                    return;
                }

                FactionSystem.ClientLeaderTransferOwnership(memberName);

                // ReSharper disable once AccessToModifiedClosure
                // ReSharper disable once PossibleNullReferenceException
                dialogWindow.Close(DialogResult.OK);
            }

            dialogWindow.AutoCloseOnOk = false;
        }

        private void AccessRightsAnyModificationHandler(
            NetworkSyncDictionary<FactionMemberRole, FactionMemberAccessRights> source)
        {
            this.Refresh();
        }

        private void ClientCurrentFactionMemberAddedOrRemovedHandler((FactionMemberEntry entry, bool isAdded) _)
        {
            this.Refresh();
        }

        private void ExecuteCommandCancelAllInvitations()
        {
            DialogWindow.ShowDialog(
                CoreStrings.QuestionAreYouSure,
                CoreStrings.Faction_Applications_RejectAll,
                okAction: FactionSystem.ClientOfficerRemoveAllInvitations,
                cancelAction: () => { });
        }

        private void ExecuteCommandInvite()
        {
            var name = this.InviteeName?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            FactionSystem.ClientOfficerInviteMember(name);
            this.InviteeName = string.Empty;
            Client.UI.BlurFocus();
        }

        [SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
        private void ExecuteCommandOpenEmblemEditor()
        {
            throw new Exception("This feature is disabled");

#pragma warning disable 162
            var viewModelFactionEmblemEditor = new ViewModelFactionEmblemEditor()
            {
                CurrentEmblem = this.factionPublicState.Emblem
            };

            var emblemEditor = new FactionEmblemEditor
            {
                DataContext = viewModelFactionEmblemEditor,
                Margin = new Thickness(0, 5, 0, 0)
            };

            DialogWindow dialogWindow = null;
            dialogWindow = DialogWindow.ShowDialog(
                title: null,
                content: emblemEditor,
                okAction: DialogOkHandler,
                cancelAction: () => { },
                okText: CoreStrings.Button_Save);
            dialogWindow.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            dialogWindow.GameWindow.Width = 530;
            dialogWindow.Window.FocusOnControl = emblemEditor;

            dialogWindow.AutoCloseOnOk = false;
            dialogWindow.Closed += DialogClosedHandler;
#pragma warning restore 162

            void DialogClosedHandler(object sender, EventArgs _)
            {
                dialogWindow.Closed -= DialogClosedHandler;
                emblemEditor.DataContext = null;
                // ReSharper disable once AccessToDisposedClosure
                viewModelFactionEmblemEditor.Dispose();
            }

            async void DialogOkHandler()
            {
                var isSuccess = await FactionSystem.ClientOfficerSetEmblem(
                                    viewModelFactionEmblemEditor.CurrentEmblem);
                if (!isSuccess)
                {
                    NotificationSystem.ClientShowNotification(
                        null,
                        CoreStrings.Faction_ErrorEmblemUsed,
                        NotificationColor.Bad);
                    return;
                }

                // ReSharper disable once AccessToModifiedClosure
                // ReSharper disable once PossibleNullReferenceException
                dialogWindow.Close(DialogResult.OK);
            }
        }

        private void ExecuteCommandOpenLeaveFactionDialog()
        {
            var stackPanel = new StackPanel();
            stackPanel.Children.Add(
                new FormattedTextBlock()
                {
                    Content = CoreStrings.Faction_DialogLeaveFaction_Message,
                    FontWeight = FontWeights.Bold,
                    Foreground = Api.Client.UI.GetApplicationResource<Brush>("BrushColorRed6")
                });

            var checkboxMessage = string.Format(CoreStrings.Faction_DialogLeaveFaction_CheckboxFormat,
                                                ClientTimeFormatHelper.FormatTimeDuration(
                                                    FactionConstants.SharedFactionJoinReturnBackCooldownDuration),
                                                ClientTimeFormatHelper.FormatTimeDuration(
                                                    FactionConstants.SharedFactionJoinCooldownDuration));

            var checkbox = new CheckBox()
            {
                Content = new FormattedTextBlock() { Content = checkboxMessage },
                Margin = new Thickness(0, 10, 0, 0)
            };

            stackPanel.Children.Add(checkbox);

            DialogWindow dialogWindow = null;
            dialogWindow = DialogWindow.ShowDialog(
                title: CoreStrings.Faction_LeaveFaction,
                content: stackPanel,
                okText: CoreStrings.Yes,
                okAction: DialogOkActionHandler,
                cancelText: CoreStrings.Button_Cancel,
                cancelAction: () => { },
                focusOnCancelButton: true,
                closeByEscapeKey: true);
            dialogWindow.AutoCloseOnOk = false;

            dialogWindow.ButtonOk.SetBinding(
                UIElement.IsEnabledProperty,
                new Binding
                {
                    Source = checkbox,
                    Path = new PropertyPath(ToggleButton.IsCheckedProperty),
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

            void DialogOkActionHandler()
            {
                if (checkbox.IsChecked != true)
                {
                    return;
                }

                FactionSystem.ClientLeaveFaction();
                // ReSharper disable once PossibleNullReferenceException
                // ReSharper disable once AccessToModifiedClosure
                dialogWindow.Close(DialogResult.OK);
            }
        }

        private void ExecuteCommandRejectAllApplications()
        {
            DialogWindow.ShowDialog(
                CoreStrings.QuestionAreYouSure,
                CoreStrings.Faction_Applications_RejectAll,
                okAction: FactionSystem.ClientOfficerRejectAllApplications,
                cancelAction: () => { });
        }

        private void FactionMembersMaxNumberChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.MembersNumberMax));
            this.NotifyPropertyChanged(nameof(this.MembersNumberText));
        }

        private void FactionUpgradeCostsChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.FactionLevelUpgradeCostLearningPoints));
        }

        private void IncomingFactionAllianceRequestsOnClientAnyModification(
            NetworkSyncDictionary<string,
                FactionAllianceRequest> source)
        {
            this.NotifyPropertyChanged(nameof(this.IncomingAllianceRequestsCount));
        }

        private void LandClaimAreaAddedOrRemovedHandler(ILogicObject obj)
        {
            this.NotifyPropertyChanged(nameof(this.FactionLandClaimsNumberText));
        }

        private void LearningPointsChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.CanUpgradeFactionLevel));
        }

        private void ReceivedApplicationsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.NotifyPropertyChanged(nameof(this.ReceivedApplicationsCount));
        }

        private void Refresh()
        {
            this.NotifyPropertyChanged(nameof(this.CurrentPlayerIsLeader));
            this.NotifyPropertyChanged(nameof(this.CurrentPlayerRoleTitle));
            this.NotifyPropertyChanged(nameof(this.CurrentPlayerRolePermissionsText));
            this.NotifyPropertyChanged(nameof(this.HasOfficerAccessRightEditDescription));
            this.NotifyPropertyChanged(nameof(this.HasOfficerAccessRightRecruitment));
            this.NotifyPropertyChanged(nameof(this.HasOfficerAccessRightDiplomacyManagement));
            this.NotifyPropertyChanged(nameof(this.IncomingAllianceRequestsCount));
            this.NotifyPropertyChanged(nameof(this.FactionLandClaimsNumberText));
        }
    }
}