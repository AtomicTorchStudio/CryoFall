namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills
{
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Skills.Data;

    public partial class WindowSkills : BaseWindowMenu
    {
        private ViewModelWindowSkills viewModel;

        public static void OpenAndSelectSkill(IProtoSkill skill)
        {
            var window = Menu.Find<WindowSkills>();
            if (!window.IsOpened)
            {
                window.Toggle();
            }

            window.viewModel?.SelectSkill(skill);
        }

        protected override void DisposeMenu()
        {
            base.DisposeMenu();
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        protected override void InitMenu()
        {
            this.DataContext = this.viewModel = new ViewModelWindowSkills();
        }

        protected override void WindowOpening()
        {
            base.WindowOpening();
            this.viewModel.RefreshExperiencePoints();
        }
    }
}