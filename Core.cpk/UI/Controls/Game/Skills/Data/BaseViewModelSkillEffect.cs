namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills.Data
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public abstract class BaseViewModelSkillEffect : BaseViewModel
    {
        private bool? isActive;

        protected BaseViewModelSkillEffect(byte level, byte maxLevel)
        {
            this.Level = level;
            this.MaxLevel = maxLevel;
        }

        protected BaseViewModelSkillEffect(ISkillEffect effect, byte maxLevel)
            : this(effect.Level, maxLevel)
        {
        }

        protected BaseViewModelSkillEffect(string description, byte level, bool isActive)
        {
            this.Description = description;
            this.Level = level;
            this.MaxLevel = 20;
            this.IsActive = isActive;
        }

        public string Description { get; protected set; }

        public bool IsActive
        {
            get => this.isActive ?? false;
            protected set
            {
                if (this.isActive == value)
                {
                    return;
                }

                this.isActive = value;
                this.VisibilityIsActive = !value ? Visibility.Collapsed : Visibility.Visible;
                this.VisibilityIsNotActive = value ? Visibility.Collapsed : Visibility.Visible;
                this.NotifyThisPropertyChanged();
            }
        }

        public byte Level { get; }

        public byte MaxLevel { get; }

        public Visibility Visibility { get; set; } = Visibility.Visible;

        public Visibility VisibilityIsActive { get; private set; } = Visibility.Visible;

        public Visibility VisibilityIsNotActive { get; private set; } = Visibility.Collapsed;

        public static BaseViewModelSkillEffect Create(ISkillEffect skillEffect, IProtoSkill protoSkill)
        {
            if (skillEffect is StatEffect statEffect)
            {
                return new ViewModelSkillEffectStat(statEffect, protoSkill.MaxLevel);
            }

            if (skillEffect is IFlagEffect flagEffect)
            {
                return new ViewModelSkillEffectFlag(flagEffect, protoSkill.MaxLevel);
            }

            throw new Exception("Unknown skill effect type: " + skillEffect);
        }

        public abstract void Refresh(byte currentLevel);
    }
}