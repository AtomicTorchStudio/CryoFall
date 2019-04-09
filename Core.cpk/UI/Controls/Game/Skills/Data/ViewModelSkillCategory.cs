namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelSkillCategory : BaseViewModel
    {
        public Action OnVisibilityChanged;

        private Visibility visibility;

        public ViewModelSkillCategory(ProtoSkillCategory category)
        {
            this.ProtoSkillCategory = category;
            this.Title = category.Name;
        }

        /// <summary>
        /// WPF design-time only constructor.
        /// </summary>
        public ViewModelSkillCategory(string title, List<ViewModelSkill> skills)
        {
            this.Title = title;
            this.Skills = skills;
        }

        public ProtoSkillCategory ProtoSkillCategory { get; }

        public IReadOnlyList<ViewModelSkill> Skills { get; private set; } = new ViewModelSkill[0];

        public string Title { get; }

        public Visibility Visibility
        {
            get => this.visibility;
            private set
            {
                if (this.visibility == value)
                {
                    return;
                }

                this.visibility = value;
                this.NotifyThisPropertyChanged();
                this.OnVisibilityChanged?.Invoke();
            }
        }

        public void UpdateSkills(
            List<IProtoSkill> newSkills,
            IReadOnlyDictionary<IProtoSkill, SkillLevelData> skillsDictionary)
        {
            this.Skills.Select(s => s.ProtoSkill)
                .ToList()
                .GetDiff(newSkills, out var diffAddedItems, out var diffRemovedItems);

            if (diffAddedItems.Count == 0
                && diffRemovedItems.Count == 0)
            {
                // no difference
                return;
            }

            // create a new list from already existing list (and overwrite this.Skills property later)
            // simple notification will be not enough
            var skills = new List<ViewModelSkill>(this.Skills);

            foreach (var removed in diffRemovedItems)
            {
                var viewModelSkill = skills.First(vm => vm.ProtoSkill == removed);
                skills.Remove(viewModelSkill);
                viewModelSkill.OnVisibilityChange -= this.OnSkillVisibilityChange;
                viewModelSkill.Dispose();
            }

            foreach (var added in diffAddedItems)
            {
                var skillLevelData = skillsDictionary[added];
                var viewModelSkill = new ViewModelSkill(added, skillLevelData);
                viewModelSkill.OnVisibilityChange += this.OnSkillVisibilityChange;
                skills.Add(viewModelSkill);
            }

            skills.Sort();

            //// uncomment to test scrollbar
            //skills.AddRange(skills);
            //skills.AddRange(skills);

            this.Skills = skills;

            // refresh visibility
            this.OnSkillVisibilityChange();
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.DisposeCollection(this.Skills);
        }

        private void OnSkillVisibilityChange()
        {
            // if any skills view model is visible - the category is visible
            foreach (var viewModelSkill in this.Skills)
            {
                if (viewModelSkill.Visibility == Visibility.Visible)
                {
                    this.Visibility = Visibility.Visible;
                    return;
                }
            }

            this.Visibility = Visibility.Collapsed;
        }
    }
}