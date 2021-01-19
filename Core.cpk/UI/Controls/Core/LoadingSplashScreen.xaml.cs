namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public partial class LoadingSplashScreen : BaseUserControl
    {
        private string currentControlState;

        private bool isFirstTimeLoaded = true;

        private Grid layoutRoot;

        private Storyboard storyboardHide;

        private Storyboard storyboardShow;

        private ViewModelLoadingSplashScreen viewModel;

        public event Action HideAnimationCompleted;

        public event Action ShowAnimationCompleted;

        public bool DisplayStructureInfos { get; set; }

        public void Refresh()
        {
            if (!this.isLoaded)
            {
                return;
            }

            var useTransitions = true;
            if (this.isFirstTimeLoaded)
            {
                this.isFirstTimeLoaded = false;
                useTransitions = false;
            }

            var state = LoadingSplashScreenManager.Instance.CurrentState;
            string controlState;
            if (state == LoadingSplashScreenState.Showing
                || state == LoadingSplashScreenState.Shown)
            {
                controlState = "Shown";
            }
            else
            {
                controlState = "Hidden";
            }

            if (this.currentControlState == controlState)
            {
                Api.Logger.Important("Already in state: " + this.currentControlState);
                return;
            }

            this.currentControlState = controlState;
            VisualStateManager.GoToElementState(this.layoutRoot, controlState, useTransitions);

            if (state == LoadingSplashScreenState.Showing
                || state == LoadingSplashScreenState.Shown)
            {
                this.viewModel.DisplayStructureInfos = this.DisplayStructureInfos;
                if (this.DisplayStructureInfos)
                {
                    this.viewModel.RandomizeInfo();
                }
            }

            Api.Logger.Important(
                $"Loading splash screen: go to state {controlState} with{(useTransitions ? " transitions" : "out any transitions")}");
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
            var visualStateGroupCollection = VisualStateManager.GetVisualStateGroups(this.layoutRoot);
            var group = (VisualStateGroup)visualStateGroupCollection[0];
            var stateHidden = group.FindState("Hidden");
            var stateShown = group.FindState("Shown");
            this.storyboardHide = stateHidden.Storyboard;
            this.storyboardShow = stateShown.Storyboard;
        }

        protected override void OnLoaded()
        {
            this.storyboardHide.Completed += this.StoryboardHideCompletedHandler;
            this.storyboardShow.Completed += this.StoryboardShowCompletedHandler;

            this.DataContext = this.viewModel = new ViewModelLoadingSplashScreen();
            this.Refresh();

            this.UpdateLayout();
        }

        protected override void OnUnloaded()
        {
            this.storyboardHide.Completed -= this.StoryboardHideCompletedHandler;
            this.storyboardShow.Completed -= this.StoryboardShowCompletedHandler;

            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private void StoryboardHideCompletedHandler(object sender, EventArgs e)
        {
            this.HideAnimationCompleted?.Invoke();
        }

        private void StoryboardShowCompletedHandler(object sender, EventArgs e)
        {
            this.ShowAnimationCompleted?.Invoke();
        }
    }
}