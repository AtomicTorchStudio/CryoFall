namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills.Data
{
    using AtomicTorch.CBND.CoreMod.Skills;

    public class ViewModelSkillEffectFlag : BaseViewModelSkillEffect
    {
        public ViewModelSkillEffectFlag(IFlagEffect flagEffect, byte maxLevel) : base(flagEffect, maxLevel)
        {
            this.Description = flagEffect.Description;
        }

        public ViewModelSkillEffectFlag(string description, byte level, bool isActive) : base(
            description,
            level,
            isActive)
        {
        }

        public override void Refresh(byte currentLevel)
        {
            this.IsActive = currentLevel >= this.Level;
        }
    }
}