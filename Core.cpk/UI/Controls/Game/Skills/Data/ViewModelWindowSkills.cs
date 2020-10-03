namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelWindowSkills : BaseViewModel
    {
        private readonly NetworkSyncDictionary<IProtoSkill, SkillLevelData> skillsDictionary;

        private ViewModelSkill selectedSkill;

        public ViewModelWindowSkills()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.skillsDictionary = Client.Characters.CurrentPlayerCharacter.SharedGetSkills().Skills;
            this.skillsDictionary.ClientPairSet += this.SkillsDictionaryPairSetHandler;
            this.skillsDictionary.ClientPairRemoved += this.SkillsDictionaryPairRemovedHandler;
            this.skillsDictionary.ClientDictionaryClear += this.SkillsDictionaryDictionaryClearHandler;
            var skillCategories = Api.FindProtoEntities<ProtoSkillCategory>()
                                     .OrderBy(category => category.Order)
                                     .Select(
                                         category => new ViewModelSkillCategory(
                                             category,
                                             onCategoryVisibilityChanged: this
                                                 .SkillSkillOrCategoryVisibilityChangedHandler,
                                             onSkillVisibilityChanged:
                                             this.SkillSkillOrCategoryVisibilityChangedHandler));

            //// uncomment to test scrollbar
            //skillCategories = skillCategories.Concat(skillCategories);

            this.SkillCategories = skillCategories.ToList();
            this.SkillsCountTotal = Api.FindProtoEntities<IProtoSkill>().Count;
            this.RefreshSkillsList();
        }

        public Brush BrushDontHasSkillsImage
            => Api.Client.UI.GetTextureBrush(
                Api.GetProtoEntity<SkillLearning>().Picture);

        public bool HasSkills => this.SkillCategories.Any(sc => sc.Visibility == Visibility.Visible);

        public ViewModelSkill SelectedSkill
        {
            get => this.selectedSkill;
            set
            {
                if (this.selectedSkill == value)
                {
                    return;
                }

                if (this.selectedSkill is not null)
                {
                    this.selectedSkill.IsSelected = false;
                }

                // first, reset to null
                // this is required to resolve SelectedItem of all the listboxes
                this.selectedSkill = null;
                this.NotifyThisPropertyChanged();

                if (value is not null
                    && value.Visibility != Visibility.Visible)
                {
                    // cannot select invisible skill
                    return;
                }

                this.selectedSkill = value;
                this.NotifyThisPropertyChanged();

                if (this.selectedSkill is not null)
                {
                    this.selectedSkill.IsSelected = true;
                }
            }
        }

        public IReadOnlyList<ViewModelSkillCategory> SkillCategories { get; }

        public int SkillsCountActive => this.SkillCategories.Sum(sc => sc.Skills.Count(s => s.Level > 0));

        public int SkillsCountTotal { get; } = 10;

        public void RefreshExperiencePointsForAllSkills()
        {
            // Refresh the experience points for all the skills that have visible experience.
            // So player will see an actual data in large and small experience bars.
            TryRefresh(this.selectedSkill); // refresh the selected skill first

            foreach (var skillCategory in this.SkillCategories)
            {
                foreach (var viewModelSkill in skillCategory.Skills)
                {
                    if (ReferenceEquals(viewModelSkill, this.selectedSkill))
                    {
                        continue;
                    }

                    TryRefresh(viewModelSkill);
                }
            }

            static void TryRefresh(ViewModelSkill viewModelSkill)
            {
                if (viewModelSkill is null
                    || viewModelSkill.Visibility != Visibility.Visible
                    || viewModelSkill.VisibilityExperience != Visibility.Visible)
                {
                    return;
                }

                viewModelSkill.RefreshExperiencePoints();
            }
        }

        public void SelectSkill(IProtoSkill skill)
        {
            foreach (var skillCategory in this.SkillCategories)
            {
                foreach (var otherSkill in skillCategory.Skills)
                {
                    if (otherSkill.ProtoSkill == skill)
                    {
                        this.SelectedSkill = otherSkill;
                        return;
                    }
                }
            }
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            if (this.skillsDictionary is null)
            {
                return;
            }

            this.skillsDictionary.ClientPairSet -= this.SkillsDictionaryPairSetHandler;
            this.skillsDictionary.ClientPairRemoved -= this.SkillsDictionaryPairRemovedHandler;
            this.skillsDictionary.ClientDictionaryClear -= this.SkillsDictionaryDictionaryClearHandler;
        }

        private void RefreshDisplays()
        {
            this.RefreshSelectedSkill();
            this.NotifyPropertyChanged(nameof(this.SkillsCountActive));
            this.NotifyPropertyChanged(nameof(this.HasSkills));
        }

        private void RefreshSelectedSkill()
        {
            if (this.selectedSkill is null
                || this.selectedSkill.IsDisposed
                || this.selectedSkill.Visibility != Visibility.Visible)
            {
                // no skill selected
                this.SelectedSkill = this.SkillCategories.SelectMany(s => s.Skills)
                                         .FirstOrDefault(s => s.Visibility == Visibility.Visible);
            }
        }

        private void RefreshSkillsList()
        {
            var actualSkillsGroupped = this.skillsDictionary.Keys.GroupBy(p => p.Category)
                                           .ToDictionary(p => p.Key, p => p.ToList());

            foreach (var viewModelCategory in this.SkillCategories)
            {
                var category = viewModelCategory.ProtoSkillCategory;
                if (!actualSkillsGroupped.TryGetValue(category, out var newSkills))
                {
                    // no such actual skills
                    if (viewModelCategory.Skills.Count == 0)
                    {
                        // no such skills in view model - no need to update
                        continue;
                    }

                    newSkills = new List<IProtoSkill>();
                }

                viewModelCategory.UpdateSkills(newSkills, this.skillsDictionary);
            }

            this.RefreshDisplays();
        }

        private void SkillsDictionaryDictionaryClearHandler(
            NetworkSyncDictionary<IProtoSkill, SkillLevelData> source)
        {
            this.RefreshSkillsList();
        }

        private void SkillsDictionaryPairRemovedHandler(
            NetworkSyncDictionary<IProtoSkill, SkillLevelData> source,
            IProtoSkill key)
        {
            this.RefreshSkillsList();
        }

        private void SkillsDictionaryPairSetHandler(
            NetworkSyncDictionary<IProtoSkill, SkillLevelData> source,
            IProtoSkill key,
            SkillLevelData value)
        {
            this.RefreshSkillsList();
        }

        private void SkillSkillOrCategoryVisibilityChangedHandler()
        {
            this.RefreshDisplays();
        }
    }
}