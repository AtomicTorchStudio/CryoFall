namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills.Data
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
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
                this.SkillCategories = this.GenerateDesignTimeCategories();
                this.selectedSkill = this.SkillCategories[0].Skills[0];
                return;
            }

            this.skillsDictionary = Client.Characters.CurrentPlayerCharacter.SharedGetSkills().Skills;
            this.skillsDictionary.ClientPairSet += this.SkillsDictionaryPairSetHandler;
            this.skillsDictionary.ClientPairRemoved += this.SkillsDictionaryPairRemovedHandler;
            this.skillsDictionary.ClientDictionaryClear += this.SkillsDictionaryDictionaryClearHandler;
            var skillCategories = Api.FindProtoEntities<ProtoSkillCategory>()
                                     .OrderBy(c => c.Order)
                                     .Select(
                                         c => new ViewModelSkillCategory(c)
                                         {
                                             OnVisibilityChanged = this.SkillCategoryVisibilityChangedHandler
                                         });

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

                if (this.selectedSkill != null)
                {
                    this.selectedSkill.IsSelected = false;
                }

                // first, reset to null
                // this is required to resolve SelectedItem of all the listboxes
                this.selectedSkill = null;
                this.NotifyThisPropertyChanged();

                if (value != null
                    && value.Visibility != Visibility.Visible)
                {
                    // cannot select invisible skill
                    return;
                }

                this.selectedSkill = value;
                this.NotifyThisPropertyChanged();

                if (this.selectedSkill != null)
                {
                    this.selectedSkill.IsSelected = true;
                }
            }
        }

        public IReadOnlyList<ViewModelSkillCategory> SkillCategories { get; }

        public int SkillsCountActive => this.SkillCategories.Sum(sc => sc.Skills.Count(s => s.Level > 0));

        public int SkillsCountTotal { get; } = 10;

        public void RefreshExperiencePoints()
        {
            foreach (var skillCategory in this.SkillCategories)
            {
                foreach (var skill in skillCategory.Skills)
                {
                    skill.RefreshExperiencePoints();
                }
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
            if (this.skillsDictionary == null)
            {
                return;
            }

            this.skillsDictionary.ClientPairSet -= this.SkillsDictionaryPairSetHandler;
            this.skillsDictionary.ClientPairRemoved -= this.SkillsDictionaryPairRemovedHandler;
            this.skillsDictionary.ClientDictionaryClear -= this.SkillsDictionaryDictionaryClearHandler;
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        private IReadOnlyList<ViewModelSkillCategory> GenerateDesignTimeCategories()
        {
            return new List<ViewModelSkillCategory>()
            {
                new ViewModelSkillCategory(
                    "Test category 1",
                    new List<ViewModelSkill>()
                    {
                        new ViewModelSkill("Test skill 1", 5,  2500, 5000),
                        new ViewModelSkill("Test skill 2", 10, 1000, 7000),
                        new ViewModelSkill("Test skill 3", 15, 8000, 10000)
                    }),
                new ViewModelSkillCategory(
                    "Test category 2",
                    new List<ViewModelSkill>()
                    {
                        new ViewModelSkill("Test skill 4", 1,  1,    500),
                        new ViewModelSkill("Test skill 5", 18, 9500, 1000)
                    })
            };
        }

        private void RefreshDisplays()
        {
            this.RefreshSelectedSkill();
            this.NotifyPropertyChanged(nameof(this.SkillsCountActive));
            this.NotifyPropertyChanged(nameof(this.HasSkills));
        }

        private void RefreshSelectedSkill()
        {
            if (this.selectedSkill == null
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

        private void SkillCategoryVisibilityChangedHandler()
        {
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
    }
}