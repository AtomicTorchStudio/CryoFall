namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.EndGame
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.EndGame.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Extras.Credits;
    using AtomicTorch.CBND.GameApi.Data.State;
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

        private Storyboard storyboardFadeIn;

        private Storyboard storyboardFadeOut;

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

                Api.Logger.Info("MenuEndGame IsDisplayed: " + value);

                if (!value)
                {
                    if (instance is not null)
                    {
                        Api.Client.UI.LayoutRootChildren.Remove(instance);
                        instance = null;
                    }

                    return;
                }

                if (instance is not null)
                {
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
            this.storyboardFadeIn = this.layoutRoot.GetResource<Storyboard>("StoryboardFadeIn");
            this.storyboardFadeOut = this.layoutRoot.GetResource<Storyboard>("StoryboardFadeOut");
            this.storyboardCreditsStart = this.layoutRoot.GetResource<Storyboard>("StoryboardCreditsStart");
            this.storyboardCreditsFinish = this.layoutRoot.GetResource<Storyboard>("StoryboardCreditsFinish");
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelMenuEndGame();

            ClientUpdateHelper.UpdateCallback += this.Update;
            this.menuCredits.ScrollFinished += this.MenuCreditsScrollFinishedHandler;
            this.storyboardFadeOut.Completed += this.StoryboardFadeOutCompletedHandler;
            Api.Client.CurrentGame.ConnectionStateChanged += CurrentGameConnectionStateChangedHandler;

            if (LoadingSplashScreenManager.Instance.CurrentState == LoadingSplashScreenState.Hidden)
            {
                this.storyboardFadeIn.Begin(this.layoutRoot);
            }

            this.storyboardCreditsStart.Begin(this.layoutRoot);

            PlayerCharacter.GetPrivateState(ClientCurrentCharacterHelper.Character)
                           .ClientSubscribe(_ => _.IsDespawned,
                                            this.CharacterDespawnedChangedHandler,
                                            this.viewModel);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;

            ClientUpdateHelper.UpdateCallback -= this.Update;
            this.menuCredits.ScrollFinished -= this.MenuCreditsScrollFinishedHandler;
            this.storyboardFadeOut.Completed -= this.StoryboardFadeOutCompletedHandler;
            Api.Client.CurrentGame.ConnectionStateChanged -= CurrentGameConnectionStateChangedHandler;
        }

        private static void CurrentGameConnectionStateChangedHandler()
        {
            IsDisplayed = false;
        }

        private void CharacterDespawnedChangedHandler(bool isDespawned)
        {
            if (!isDespawned)
            {
                this.storyboardFadeIn.Stop(this.layoutRoot);
                this.storyboardFadeOut.Begin(this.layoutRoot);
            }
        }

        private void MenuCreditsScrollFinishedHandler()
        {
            ((Panel)this.menuCredits.Parent)?.Children.Remove(this.menuCredits);
            this.storyboardCreditsStart.Stop(this.layoutRoot);
            this.storyboardCreditsFinish.Begin(this.layoutRoot);
        }

        private void StoryboardFadeOutCompletedHandler(object? sender, EventArgs e)
        {
            IsDisplayed = false;
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