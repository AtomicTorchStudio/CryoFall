namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Login
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Login.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuLogin : BaseUserControl
    {
        private static MenuLogin instance;

        private static ViewModelMenuLogin viewModel;

        public static event Action IsDisplayedChanged;

        public static bool IsDisplayed
        {
            get => instance is not null;
            private set
            {
                if (IsDisplayed == value)
                {
                    return;
                }

                try
                {
                    if (value)
                    {
                        // must be displayed
                        if (instance is not null)
                        {
                            // already displayed
                            return;
                        }

                        // create new instance and add into layout root
                        viewModel = new ViewModelMenuLogin();
                        instance = new MenuLogin();
                        Api.Client.UI.LayoutRootChildren.Add(instance);
                        return;
                    }

                    // must be hidden
                    if (instance is null)
                    {
                        // already hidden
                        return;
                    }

                    // hide
                    Api.Client.UI.LayoutRootChildren.Remove(instance);
                    instance = null;
                    viewModel?.Dispose();
                    viewModel = null;
                }
                finally
                {
                    IsDisplayedChanged?.Invoke();
                }
            }
        }

        public static void SetDisplayed(MenuLoginMode mode)
        {
            if (mode == MenuLoginMode.None)
            {
                IsDisplayed = false;
                return;
            }

            IsDisplayed = true;
            viewModel.Setup(mode);
        }

        protected override void OnLoaded()
        {
            MainMenuOverlay.DestroyIfPresent();
            this.DataContext = viewModel;

            if (!IsDesignTime)
            {
                Api.Client.MasterServer.Connect();
            }
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
        }
    }
}