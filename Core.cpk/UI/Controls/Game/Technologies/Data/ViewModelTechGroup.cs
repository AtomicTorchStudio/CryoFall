namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelTechGroup : BaseViewModel
    {
        private IReadOnlyList<BaseViewModelTechGroupRequirement> requirements;

        public ViewModelTechGroup(TechGroup techGroup)
        {
            this.TechGroup = techGroup;
            this.Icon = Client.UI.GetTextureBrush(this.TechGroup.Icon);

            this.TechGroup.NodesChanged += this.TechNodesChangedHandler;
            ClientComponentTechnologiesWatcher.TechGroupsChanged += this.TechGroupsChangedHandler;
            ClientComponentTechnologiesWatcher.TechNodesChanged += this.TechNodesChangedHandler;
            ClientComponentTechnologiesWatcher.LearningPointsChanged += this.LearningPointsChangedHandler;

            this.Refresh();
        }

        public bool CanUnlock { get; private set; }

        public string Description => this.TechGroup.Description;

        public Brush Icon { get; }

        public bool IsSelected { get; set; }

        public bool IsUnlocked { get; private set; }

        public ushort LearningPointsPrice => this.TechGroup.LearningPointsPrice;

        public int NodesTotalCount => this.TechGroup.Nodes.Count;

        public int NodesUnlockedCount { get; private set; }

        public IReadOnlyList<BaseViewModelTechGroupRequirement> Requirements
        {
            get
            {
                if (this.requirements == null)
                {
                    var list = new List<BaseViewModelTechGroupRequirement>(this.TechGroup.GroupRequirements.Count);
                    foreach (var requirement in this.TechGroup.GroupRequirements)
                    {
                        list.Add(requirement.CreateViewModel());
                    }

                    this.requirements = list;
                }

                return this.requirements;
            }
        }

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
                var canUnlock = this.TechGroup.SharedCanUnlock(Client.Characters.CurrentPlayerCharacter, out _);
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

            this.TechGroup.NodesChanged -= this.TechNodesChangedHandler;
            ClientComponentTechnologiesWatcher.TechGroupsChanged -= this.TechGroupsChangedHandler;
            ClientComponentTechnologiesWatcher.TechNodesChanged -= this.TechNodesChangedHandler;
            ClientComponentTechnologiesWatcher.LearningPointsChanged -= this.LearningPointsChangedHandler;
        }

        private void LearningPointsChangedHandler()
        {
            this.Refresh();
        }

        private void TechGroupsChangedHandler()
        {
            this.Refresh();
        }

        private void TechNodesChangedHandler()
        {
            this.Refresh();
        }
    }
}