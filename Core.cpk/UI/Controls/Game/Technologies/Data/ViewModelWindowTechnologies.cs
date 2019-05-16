namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Timer;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterRespawn;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelWindowTechnologies : BaseViewModel
    {
        public const string DialogDoYouWantToResearch = "Do you want to research {0}?";

        private static readonly SoundUI GroupSelectedSoundUI
            = Api.Client.UI.GetApplicationResource<SoundUI>("SoundListElementClick");

        private readonly PlayerCharacterTechnologies technologies;

        private ViewModelTechGroup lastGroupSelected;

        private ViewModelTechGroup listSelectedTechGroup;

        private ViewModelTechGroup selectedTechGroup;

        private ViewModelTechTier selectedTier;

        public ViewModelWindowTechnologies()
        {
            this.CommandCloseDetails = new ActionCommand(this.CloseDetails);

            var tiers = new List<ViewModelTechTier>();
            for (var tierIndex = (byte)TechConstants.MinTier; tierIndex <= (byte)TechConstants.MaxTier; tierIndex++)
            {
                tiers.Add(new ViewModelTechTier((TechTier)tierIndex));
            }

            this.Tiers = tiers;
            this.SelectedTier = tiers[0];

            this.technologies = ClientComponentTechnologiesWatcher.CurrentTechnologies;
            this.technologies.ClientSubscribe(
                _ => _.LearningPoints,
                _ => this.NotifyPropertyChanged(nameof(this.LearningPoints)),
                this);

            this.technologies.Groups.ClientElementInserted += this.GroupInserted;

            ClientCurrentCharacterHelper.PrivateState.ClientSubscribe(
                _ => _.FinalStatsCache,
                _ => this.NotifyPropertyChanged(
                    nameof(this.LearningPointsRetainedAfterDeath)),
                this);
        }

        public BaseCommand CommandCloseDetails { get; }

        public int LearningPoints => this.technologies.LearningPoints;

        public ushort LearningPointsRetainedAfterDeath
            => ServerCharacterDeathMechanic.SharedGetLearningPointsRetainedAfterDeath(
                Client.Characters.CurrentPlayerCharacter);

        public ViewModelTechGroup ListSelectedTechGroup
        {
            get => this.listSelectedTechGroup;
            set
            {
                if (this.listSelectedTechGroup == value)
                {
                    return;
                }

                this.listSelectedTechGroup = value;
                this.listSelectedTechGroup?.Refresh();

                this.NotifyThisPropertyChanged();

                if (this.listSelectedTechGroup == null)
                {
                    this.SelectedTechGroup = null;
                    return;
                }

                this.lastGroupSelected = value;

                // HACK: play button click sounds - actual click sound will be never played
                // (it's never triggered when the item selected in the ListBox)
                SoundUI.PlaySound(GroupSelectedSoundUI);

                if (this.listSelectedTechGroup.IsUnlocked)
                {
                    this.SelectedTechGroup = this.listSelectedTechGroup;
                    return;
                }

                // the group is locked
                this.SelectedTechGroup = null;

                // apply hack - reset selected tech group at next frame
                ClientComponentTimersManager.AddAction(
                    0,
                    () => this.ListSelectedTechGroup = null);

                if (!this.listSelectedTechGroup?.CanUnlock ?? false)
                {
                    NotificationSystem.ClientShowNotification(
                        TechnologiesSystem.NotificationCannotUnlockTech,
                        message: this.listSelectedTechGroup
                                     .Requirements
                                     .Where(r => !r.IsSatisfied)
                                     .Select(r => r.GetErrorMessage())
                                     .GetJoinedString("[br]"),
                        color: NotificationColor.Bad,
                        icon: this.listSelectedTechGroup.TechGroup.Icon);
                    return;
                }

                // can unlock group
                this.DisplayUnlockGroupDialog(this.listSelectedTechGroup);
            }
        }

        public ViewModelTechGroup SelectedTechGroup
        {
            get => this.selectedTechGroup;
            private set
            {
                if (this.selectedTechGroup == value)
                {
                    return;
                }

                if (this.selectedTechGroup != null)
                {
                    this.selectedTechGroup.IsSelected = false;
                }

                this.selectedTechGroup = value;

                if (this.selectedTechGroup != null)
                {
                    this.selectedTechGroup.IsSelected = true;
                }

                this.VisibilityTiersAndGroups = this.selectedTechGroup == null
                                                    ? Visibility.Visible
                                                    : Visibility.Collapsed;

                this.VisibilityGroupDetails = this.selectedTechGroup != null
                                                  ? Visibility.Visible
                                                  : Visibility.Collapsed;

                this.NotifyThisPropertyChanged();
            }
        }

        public ViewModelTechTier SelectedTier
        {
            get => this.selectedTier;
            set
            {
                if (this.selectedTier == value)
                {
                    return;
                }

                if (this.selectedTier != null)
                {
                    this.selectedTier.IsSelected = false;
                }

                this.selectedTier = value;

                if (this.selectedTier != null)
                {
                    this.selectedTier.IsSelected = true;
                }

                this.NotifyThisPropertyChanged();
            }
        }

        public IList<ViewModelTechTier> Tiers { get; }

        public Visibility VisibilityGroupDetails { get; private set; } = Visibility.Collapsed;

        public Visibility VisibilityTiersAndGroups { get; private set; } = Visibility.Visible;

        protected override void DisposeViewModel()
        {
            if (IsDesignTime)
            {
                return;
            }

            base.DisposeViewModel();
            this.technologies.Groups.ClientElementInserted -= this.GroupInserted;
        }

        private void CloseDetails()
        {
            this.ListSelectedTechGroup = null;
        }

        private void DisplayUnlockGroupDialog(ViewModelTechGroup techGroup)
        {
            DialogWindow.ShowDialog(
                CoreStrings.QuestionAreYouSure,
                new TextBlock()
                {
                    Text = string.Format(DialogDoYouWantToResearch, techGroup.Title),
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MaxWidth = 400
                },
                okText: CoreStrings.Yes,
                cancelText: CoreStrings.No,
                okAction: () => TechnologiesSystem.ClientUnlockGroup(techGroup.TechGroup),
                cancelAction: () => { });
        }

        private void GroupInserted(NetworkSyncList<TechGroup> source, int index, TechGroup value)
        {
            this.OnGroupSetOrInserted(value);
        }

        private void OnGroupSetOrInserted(TechGroup value)
        {
            if (this.lastGroupSelected?.TechGroup == value)
            {
                // added group which was selected last time
                this.ListSelectedTechGroup = this.lastGroupSelected;
            }
        }
    }
}