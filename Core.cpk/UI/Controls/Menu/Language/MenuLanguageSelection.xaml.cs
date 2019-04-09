namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Language
{
    using System;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Language.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class MenuLanguageSelection : BaseUserControl
    {
        private static MenuLanguageSelection instance;

        private static ViewModelMenuLanguageSelection viewModel;

        public static event Action IsDisplayedChanged;

        public static bool IsDisplayed
        {
            get => instance != null;
            set
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
                        if (instance != null)
                        {
                            // already displayed
                            return;
                        }

                        // create new instance and add into layout root
                        viewModel = new ViewModelMenuLanguageSelection();
                        instance = new MenuLanguageSelection();
                        Api.Client.UI.LayoutRootChildren.Add(instance);

                        if (viewModel.Languages.Count == 1)
                        {
                            // auto-select the only available language
                            viewModel.CommandAccept.Execute(null);
                        }

                        return;
                    }

                    // must be hidden
                    if (instance == null)
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

        protected override void OnLoaded()
        {
            this.DataContext = viewModel;
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
        }
    }
}