﻿namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelTechGroup : BaseViewModel
    {
        public ViewModelTechGroup(TechGroup techGroup)
        {
            this.TechGroup = techGroup;

            var requirements = new List<BaseViewModelTechGroupRequirement>(this.TechGroup.GroupRequirements.Count);
            foreach (var requirement in this.TechGroup.GroupRequirements)
            {
                if (requirement is TechGroupRequirementLearningPoints)
                {
                    // do not display such requirement as we display it separately now
                    continue;
                }

                var viewModel = requirement.CreateViewModel();
                viewModel.SubscribePropertyChange(_ => _.IsSatisfied,
                                                  _ => this.Refresh());
                requirements.Add(viewModel);
            }

            this.Requirements = requirements;

            this.TechGroup.NodesChanged += this.Refresh;
            ClientComponentTechnologiesWatcher.TechGroupsChanged += this.Refresh;
            ClientComponentTechnologiesWatcher.TechNodesChanged += this.Refresh;
            ClientComponentTechnologiesWatcher.LearningPointsChanged += this.Refresh;

            this.Refresh();
        }

        public bool CanUnlock { get; private set; }

        public string Description => this.TechGroup.Description;

        public Brush Icon => Client.UI.GetTextureBrush(this.TechGroup.Icon);

        public bool IsSelected { get; set; }

        public bool IsUnlocked { get; private set; }

        public ushort LearningPointsPrice => this.TechGroup.LearningPointsPrice;

        public int NodesTotalCount => this.TechGroup.Nodes.Count;

        public int NodesUnlockedCount { get; private set; }

        public ushort RequiredLearningPoints => this.TechGroup.LearningPointsPrice;

        public IReadOnlyList<BaseViewModelTechGroupRequirement> Requirements { get; }

        public TechGroup TechGroup { get; }

        public string Title => this.TechGroup.Name;

        public double UnlockProgress { get; private set; }

        public Visibility VisibilityLocked { get; private set; }

        public Visibility VisibilityLockedCannotUnlock { get; private set; }

        public Visibility VisibilityLockedCanUnlock { get; private set; }

        public Visibility VisibilityRequirements { get; private set; }

        public Visibility VisibilityUnlocked { get; private set; }

        public void Refresh()
        {
            var technologies = ClientComponentTechnologiesWatcher.CurrentTechnologies;
            if (technologies is null)
            {
                // perhaps changing the game server so it's not yet obtained
                return;
            }

            var isUnlocked = technologies.SharedIsGroupUnlocked(this.TechGroup);
            this.IsUnlocked = isUnlocked;

            this.VisibilityUnlocked = isUnlocked ? Visibility.Visible : Visibility.Collapsed;
            this.VisibilityLocked = !isUnlocked ? Visibility.Visible : Visibility.Collapsed;

            if (isUnlocked)
            {
                this.CanUnlock = false;
                this.VisibilityLockedCanUnlock
                    = this.VisibilityLockedCannotUnlock
                          = Visibility.Collapsed;
                this.VisibilityRequirements = Visibility.Collapsed;

                this.NodesUnlockedCount = technologies.SharedGetUnlockedNodesCount(
                    this.TechGroup);
            }
            else
            {
                var canUnlock = this.TechGroup.SharedCanUnlock(Client.Characters.CurrentPlayerCharacter,
                                                               skipLearningPointsCheck: true,
                                                               out _);
                this.CanUnlock = canUnlock;
                this.VisibilityLockedCanUnlock = canUnlock ? Visibility.Visible : Visibility.Collapsed;
                this.VisibilityLockedCannotUnlock = !canUnlock ? Visibility.Visible : Visibility.Collapsed;
                this.VisibilityRequirements = this.TechGroup.GroupRequirements.Count > 0
                                                  ? Visibility.Visible
                                                  : Visibility.Collapsed;
            }

            this.UnlockProgress = isUnlocked
                                      ? technologies.SharedGetUnlockedNodesPercent(this.TechGroup)
                                      : 0;

            this.NotifyPropertyChanged(nameof(this.NodesTotalCount));
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();

            this.TechGroup.NodesChanged -= this.Refresh;
            ClientComponentTechnologiesWatcher.TechGroupsChanged -= this.Refresh;
            ClientComponentTechnologiesWatcher.TechNodesChanged -= this.Refresh;
            ClientComponentTechnologiesWatcher.LearningPointsChanged -= this.Refresh;
        }
    }
}