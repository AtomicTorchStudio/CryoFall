namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.Skills;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelSkill : BaseViewModel, IComparable<ViewModelSkill>
    {
        private readonly SkillLevelData data;

        private readonly IProtoSkill skill;

        private bool isSelected;

        private Visibility visibility;

        public ViewModelSkill(IProtoSkill skill, SkillLevelData data)
        {
            this.skill = skill;
            this.Title = this.skill.Name;
            this.Description = this.skill.Description;
            this.LevelMax = this.skill.MaxLevel;
            this.data = data;

            data.ClientSubscribe(
                _ => _.Level,
                _ =>
                {
                    this.RefreshLevelAndVisibility();
                    this.RequestExperienceAndRefreshAll();
                },
                this);

            this.RefreshLevelAndVisibility();
            this.RequestExperienceAndRefreshAll();
        }

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        public ViewModelSkill(
            string title,
            byte level,
            double experienceLocal,
            double experienceLocalNextLevel)
        {
            this.Title = title;
            this.Description = "Bla bla bla description text for " + title;
            this.Level = level;
            this.LevelMax = 20;
            this.ExperienceLocal = (uint)experienceLocal;
            this.ExperienceLocalNextLevel = (uint)experienceLocalNextLevel;

            this.Effects = new[]
            {
                new ViewModelSkillEffectFlag("Special effect 1", level: 5,  isActive: true),
                new ViewModelSkillEffectFlag("Special effect 2", level: 10, isActive: true),
                new ViewModelSkillEffectFlag("Special effect 3", level: 20, isActive: false)
            };
        }

        public event Action OnVisibilityChange;

        public string Description { get; }

        public IReadOnlyList<BaseViewModelSkillEffect> Effects { get; private set; }

        public uint ExperienceLocal { get; private set; }

        public uint ExperienceLocalNextLevel { get; private set; }

        public double ExperienceTotal { get; private set; }

        public double ExperienceTotalNextLevel { get; private set; }

        public IReadOnlyList<string> ExtraDescriptionEntries
            => this.skill.ExtraDescriptionEntries;

        public Brush Icon => IsDesignTime
                                 ? (Brush)Brushes.Red
                                 : Client.UI.GetTextureBrush(this.skill.Icon);

        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (this.isSelected == value)
                {
                    return;
                }

                this.isSelected = value;

                if (this.IsDisposed)
                {
                    return;
                }

                this.NotifyThisPropertyChanged();

                if (this.isSelected)
                {
                    this.RequestExperienceAndRefreshAll();
                }
            }
        }

        public byte Level { get; private set; }

        public byte LevelMax { get; }

        public Brush Picture => IsDesignTime
                                    ? (Brush)Brushes.Blue
                                    : Client.UI.GetTextureBrush(this.skill.Picture);

        public IProtoSkill ProtoSkill => this.skill;

        public ProtoSkillCategory SkillCategory => this.skill.Category;

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
                this.OnVisibilityChange?.Invoke();
                this.NotifyThisPropertyChanged();
            }
        }

        public Visibility VisibilityExperience { get; private set; } = Visibility.Visible;

        public Visibility VisibilityMaxLevelReached { get; private set; } = Visibility.Collapsed;

        public int CompareTo(ViewModelSkill other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            if (ReferenceEquals(null, other))
            {
                return 1;
            }

            return string.Compare(this.Title, other.Title, StringComparison.Ordinal);
        }

        public void RefreshExperiencePoints()
        {
            this.RequestExperienceAndRefreshAll();
        }

        private List<BaseViewModelSkillEffect> CreateEffectsList()
        {
            var list = this.skill.GetEffects()
                           .OrderBy(ef => ef.Level)
                           .Select(ef => BaseViewModelSkillEffect.Create(ef, this.skill))
                           .ToList();

            // create combined display view models for stat effects
            // (these are not rendered separately even if they're present in the list)
            // please note - we're using combined display even for the single stat effects
            var maxLevel = this.skill.MaxLevel;
            foreach (var group in list.OfType<ViewModelSkillEffectStat>()
                                      .GroupBy(ef => ef.StatEffect.StatName)
                                      .OrderByDescending(g => list.IndexOf(g.First())))
            {
                list.Insert(
                    0,
                    new ViewModelSkillEffectCombinedStats(this.skill,
                                                          group.Select(vm => vm.StatEffect).ToArray(),
                                                          maxLevel));
            }

            return list;
        }

        private void RefreshLevelAndVisibility()
        {
            this.Level = this.data.Level;
            this.Visibility = this.Level > 0 ? Visibility.Visible : Visibility.Collapsed;

            this.UpdateEffects();
        }

        private async void RequestExperienceAndRefreshAll()
        {
            var experience = await ClientSkillsDisplaySystem.ClientGetSkillExperience(this.skill);
            if (this.IsDisposed)
            {
                return;
            }

            this.RefreshLevelAndVisibility();

            this.ExperienceTotalNextLevel = this.skill.GetExperienceForLevel((byte)(this.Level + 1));
            var experienceTotalCurrentLevel = this.skill.GetExperienceForLevel(this.Level);

            this.ExperienceTotal = experience;

            this.VisibilityExperience = this.Level < this.LevelMax
                                            ? Visibility.Visible
                                            : Visibility.Collapsed;

            this.VisibilityMaxLevelReached = this.VisibilityExperience == Visibility.Visible
                                                 ? Visibility.Collapsed
                                                 : Visibility.Visible;

            if (this.VisibilityExperience == Visibility.Visible)
            {
                this.ExperienceLocal = (uint)(this.ExperienceTotal - experienceTotalCurrentLevel);
                this.ExperienceLocalNextLevel = (uint)(this.ExperienceTotalNextLevel - experienceTotalCurrentLevel);
            }
            else
            {
                this.ExperienceLocalNextLevel = this.ExperienceLocal = 1;
            }
        }

        private void UpdateEffects()
        {
            if (!this.isSelected)
            {
                return;
            }

            var level = this.Level;

            var effects = this.Effects ?? this.CreateEffectsList();
            foreach (var viewModelEffect in effects)
            {
                viewModelEffect.Refresh(level);
            }

            this.Effects ??= effects;
        }
    }
}