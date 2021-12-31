namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelNewFactionControl : BaseViewModel
    {
        public ViewModelNewFactionControl()
        {
            FactionSystem.ClientCurrentFactionChanged += this.CurrentFactionChangedHandler;
            NewbieProtectionSystem.ClientNewbieProtectionTimeRemainingReceived += this.NewbieProtectionChangedHandler;
            FactionConstants.ClientFactionCreateCostChanged += this.FactionCreateCostChangedHandler;
        }

        public string ClanTag { get; set; }

        public BaseCommand CommandCancelNewbieProtection
            => new ActionCommand(this.ExecuteCommandCancelNewbieProtection);

        public BaseCommand CommandCreateFaction
            => new ActionCommand(this.ExecuteCommandCreateFaction);

        public string CostText => string.Format(CoreStrings.LearningPointsCost_Format,
                                                FactionConstants.SharedCreateFactionCost);

        public FactionKindData[] FactionKinds => Enum.GetValues(typeof(FactionKind))
                                                     .Cast<FactionKind>()
                                                     .Select(e => new FactionKindData(e))
                                                     .Reverse()
                                                     .ToArray();

        public bool HasFaction => FactionSystem.ClientHasFaction;

        public bool IsUnderNewbieProtection => NewbieProtectionSystem.ClientIsNewbie;

        public FactionKind SelectedKind { get; set; } = FactionKind.Private;

        public string UnderNewbieProtectionText
            => NewbieProtectionSystem.Notification_CannotPerformActionWhileUnderProtection
               + "[br]"
               + NewbieProtectionSystem.Notification_CanCancelProtection;

        public ViewModelFactionEmblemEditor ViewModelFactionEmblemEditor { get; }
            = new();

        protected override void DisposeViewModel()
        {
            FactionSystem.ClientCurrentFactionChanged -= this.CurrentFactionChangedHandler;
            NewbieProtectionSystem.ClientNewbieProtectionTimeRemainingReceived -= this.NewbieProtectionChangedHandler;
            FactionConstants.ClientFactionCreateCostChanged -= this.FactionCreateCostChangedHandler;
            base.DisposeViewModel();
        }

        private void CurrentFactionChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.HasFaction));
        }

        private void ExecuteCommandCancelNewbieProtection()
        {
            DialogWindow.ShowDialog(
                CoreStrings.QuestionAreYouSure,
                NewbieProtectionSystem.Dialog_CancelNewbieProtection,
                okAction: NewbieProtectionSystem.ClientDisableNewbieProtection,
                cancelAction: () => { },
                focusOnCancelButton: true);
        }

        private void ExecuteCommandCreateFaction()
        {
            if (FactionSystem.ClientCheckIsUnderJoinCooldown(showErrorNotification: true))
            {
                return;
            }

            var clanTag = (this.ClanTag ?? string.Empty).ToUpperInvariant();
            this.ClanTag = clanTag;
            if (!FactionSystem.SharedIsValidClanTag(clanTag))
            {
                DialogWindow.ShowDialog(
                    title: CoreStrings.ClanTag_Invalid,
                    text: CoreStrings.ClanTag_Requirements,
                    closeByEscapeKey: true);
                return;
            }

            var technologies = ClientCurrentCharacterHelper.Character.SharedGetTechnologies();
            if (technologies.LearningPoints < FactionConstants.SharedCreateFactionCost)
            {
                NotificationSystem.ClientShowNotification(
                    title: null,
                    TechGroup.ErrorNotEnoughLearningPoints,
                    NotificationColor.Bad);
                return;
            }

            var textElement = DialogWindow.CreateTextElement(
                CoreStrings.Faction_CreateFaction_ConfirmationDialog2
                + "[br][br]"
                + CoreStrings.Faction_JoinCooldown_Description
                + " ("
                + FactionSystem.ClientDefaultJoinCooldownDurationText
                + ")",
                TextAlignment.Left);
            textElement.Foreground = Api.Client.UI.GetApplicationResource<Brush>("BrushColorRed6");
            textElement.FontWeight = FontWeights.Bold;

            var checkbox = new CheckBox()
            {
                Content = new FormattedTextBlock() { Content = CoreStrings.ChatDisclaimer_Checkbox },
                Margin = new Thickness(0, 10, 0, 0)
            };

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(textElement);
            stackPanel.Children.Add(checkbox);

            var dialogWindow = DialogWindow.ShowDialog(
                title: CoreStrings.Faction_CreateFaction_ConfirmationDialog1,
                stackPanel,
                okText: CoreStrings.Faction_CreateFaction,
                cancelText: CoreStrings.Button_Cancel,
                okAction: CreateFaction,
                cancelAction: () => { },
                closeByEscapeKey: true,
                focusOnCancelButton: true);

            dialogWindow.ButtonOk.SetBinding(
                UIElement.IsEnabledProperty,
                new Binding
                {
                    Source = checkbox,
                    Path = new PropertyPath(ToggleButton.IsCheckedProperty),
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });

            async void CreateFaction()
            {
                var result = await FactionSystem.ClientCreateFaction(this.SelectedKind,
                                                                     clanTag,
                                                                     this.ViewModelFactionEmblemEditor.CurrentEmblem);
                switch (result)
                {
                    case FactionSystem.CreateFactionResult.Success:
                        this.ClanTag = null;
                        return;

                    case FactionSystem.CreateFactionResult.InvalidOrTakenClanTag:
                        DialogWindow.ShowDialog(
                            title: null,
                            text: CoreStrings.ClanTag_Exists,
                            closeByEscapeKey: true);
                        break;

                    case FactionSystem.CreateFactionResult.EmblemUsed:
                        DialogWindow.ShowDialog(
                            title: null,
                            text: CoreStrings.Faction_ErrorEmblemUsed,
                            closeByEscapeKey: true);
                        break;

                    default:
                        Logger.Error("Received status: " + result);
                        return;
                }
            }
        }

        private void FactionCreateCostChangedHandler()
        {
            this.NotifyPropertyChanged(nameof(this.CostText));
        }

        private void NewbieProtectionChangedHandler(double obj)
        {
            this.NotifyPropertyChanged(nameof(this.IsUnderNewbieProtection));
        }
    }
}