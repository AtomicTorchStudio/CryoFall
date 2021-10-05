namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.EndGame
{
    using System.Windows.Controls;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.EndGame.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Extras.Credits;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuEndGame : BaseUserControl
    {
        private static MenuEndGame instance;

        private Grid layoutRoot;

        private MenuCredits menuCredits;

        private Storyboard storyboardCreditsFinish;

        private Storyboard storyboardCreditsStart;

        private ViewModelMenuEndGame viewModel;

        public static bool IsDisplayed
        {
            get => instance is not null;
            set
            {
                if (IsDisplayed == value)
                {
                    return;
                }

                if (!value)
                {
                    if (instance is not null)
                    {
                        Api.Client.UI.LayoutRootChildren.Remove(instance);
                    }

                    return;
                }

                instance = new MenuEndGame();
                Api.Client.UI.LayoutRootChildren.Add(instance);
            }
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
            this.menuCredits = this.GetByName<MenuCredits>("MenuCredits");
            this.storyboardCreditsStart = this.layoutRoot.GetResource<Storyboard>("StoryboardCreditsStart");
            this.storyboardCreditsFinish = this.layoutRoot.GetResource<Storyboard>("StoryboardCreditsFinish");
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelMenuEndGame();

            ClientUpdateHelper.UpdateCallback += this.Update;
            this.menuCredits.ScrollFinished += this.MenuCreditsScrollFinishedHandler;
            this.storyboardCreditsStart.Begin(this.layoutRoot);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;

            ClientUpdateHelper.UpdateCallback -= this.Update;
            this.menuCredits.ScrollFinished -= this.MenuCreditsScrollFinishedHandler;
            this.storyboardCreditsStart.Stop(this.layoutRoot);
            this.storyboardCreditsFinish.Stop(this.layoutRoot);
        }

        private void MenuCreditsScrollFinishedHandler()
        {
            ((Panel)this.menuCredits.Parent)?.Children.Remove(this.menuCredits);
            this.storyboardCreditsStart.Stop(this.layoutRoot);
            this.storyboardCreditsFinish.Begin(this.layoutRoot);
        }

        private void Update()
        {
            var input = Api.Client.Input;
            var isButtonHeld = input.IsKeyHeld(InputKey.Space,    evenIfHandled: true)
                               || input.IsKeyHeld(InputKey.Enter, evenIfHandled: true)
                               || ClientInputManager.IsButtonHeld(GameButton.ActionUseCurrentItem, evenIfHandled: true)
                               || ClientInputManager.IsButtonHeld(GameButton.ActionInteract,       evenIfHandled: true);
            this.menuCredits.AutoScrollSpeed = isButtonHeld ? 8.0 : 1.0;

            if (Api.Shared.IsDebug
                && input.IsKeyHeld(InputKey.Escape, evenIfHandled: true)
                && input.IsKeyHeld(InputKey.Shift,  evenIfHandled: true))
            {
                input.ConsumeKey(InputKey.Escape);
                this.storyboardCreditsFinish.SpeedRatio = 100;
                this.MenuCreditsScrollFinishedHandler();
            }
        }
    }
}