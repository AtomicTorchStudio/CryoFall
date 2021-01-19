namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Quests.QuestTracking
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.Systems.Cursor;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Quests.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using Menu = AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu.Menu;

    public partial class HUDQuestTrackingEntryControl : BaseUserControl
    {
        private Border root;

        private Storyboard storyboardFadeOut;

        private Storyboard storyboardHide;

        private Storyboard storyboardShow;

        private ViewModelQuestEntry viewModel;

        public bool IsHiding { get; private set; }

        public PlayerCharacterQuests.CharacterQuestEntry QuestEntry { get; private set; }

        public static HUDQuestTrackingEntryControl Create(PlayerCharacterQuests.CharacterQuestEntry questEntry)
        {
            return new() { QuestEntry = questEntry };
        }

        public void Hide(bool quick)
        {
            if (quick)
            {
                this.storyboardFadeOut.SpeedRatio = 6.5;
            }

            if (this.IsHiding)
            {
                // already hiding
                return;
            }

            this.IsHiding = true;

            if (!this.isLoaded)
            {
                this.RemoveControl();
                return;
            }

            this.storyboardShow.Stop();
            this.storyboardFadeOut.Begin();
        }

        protected override void InitControl()
        {
            this.storyboardShow = this.GetResource<Storyboard>("StoryboardShow");
            this.storyboardHide = this.GetResource<Storyboard>("StoryboardHide");
            this.storyboardFadeOut = this.GetResource<Storyboard>("StoryboardFadeOut");
            this.root = this.GetByName<Border>("Border");
        }

        protected override void OnLoaded()
        {
            this.viewModel = new ViewModelQuestEntry(this.QuestEntry,
                                                     callbackOnFinishedStateChanged: _ => { },
                                                     showRequirementIcons: false);
            this.DataContext = this.viewModel;
            this.UpdateLayout();
            this.viewModel.RequiredHeight = (float)this.ActualHeight;

            this.storyboardFadeOut.Completed += this.StoryboardFadeOutCompletedHandler;
            this.storyboardHide.Completed += this.StoryboardHideCompletedHandler;
            this.root.MouseLeftButtonDown += RootMouseButtonHandler;
            this.root.MouseRightButtonDown += RootMouseButtonHandler;
            this.root.MouseEnter += RootMouseEnterHandler;
            this.root.MouseLeave += RootMouseLeaveHandler;

            this.storyboardShow.Begin();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;

            this.storyboardFadeOut.Completed -= this.StoryboardFadeOutCompletedHandler;
            this.storyboardHide.Completed -= this.StoryboardHideCompletedHandler;
            this.root.MouseLeftButtonDown -= RootMouseButtonHandler;
            this.root.MouseRightButtonDown -= RootMouseButtonHandler;
            this.root.MouseEnter -= RootMouseEnterHandler;
            this.root.MouseLeave -= RootMouseLeaveHandler;

            // to ensure that the control has a hiding flag (used for ClientComponentNotificationAutoHideChecker)
            this.IsHiding = true;
        }

        private static void RootMouseButtonHandler(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            Menu.Open<WindowQuests>();
        }

        private static void RootMouseEnterHandler(object sender, MouseEventArgs e)
        {
            ClientCursorSystem.CurrentCursorId = CursorId.InteractionPossible;
        }

        private static void RootMouseLeaveHandler(object sender, MouseEventArgs e)
        {
            ClientCursorSystem.CurrentCursorId = CursorId.Default;
        }

        private void RemoveControl()
        {
            ((Panel)this.Parent).Children.Remove(this);
        }

        private void StoryboardFadeOutCompletedHandler(object sender, EventArgs e)
        {
            this.storyboardHide.Begin();
        }

        private void StoryboardHideCompletedHandler(object sender, EventArgs e)
        {
            this.RemoveControl();
        }
    }
}