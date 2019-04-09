namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.DebugTools
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.ClientOptions.General;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class DebugToolsOverlay : BaseUserControl
    {
        private static DebugToolsOverlay instance;

        private ClientInputContext inputContext;

        private FrameworkElement overlay;

        public static bool IsInstanceExist => instance != null;

        public static void Toggle()
        {
            if (IsInstanceExist)
            {
                DestroyInstance();
            }
            else
            {
                if (!GeneralOptionDeveloperMode.IsEnabled)
                {
                    // developer mode off - don't open debug tools
                    return;
                }

                CreateInstance();
            }
        }

        public static void Toggle(bool isEnabled)
        {
            if (isEnabled)
            {
                CreateInstance();
            }
            else
            {
                DestroyInstance();
            }
        }

        protected override void InitControl()
        {
            this.overlay = this.GetByName<FrameworkElement>("Overlay");
        }

        protected override void OnLoaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.DataContext = ViewModelDebugToolsOverlay.Instance;
            this.overlay.MouseLeftButtonDown += this.OverlayMouseLeftButtonDownHandler;

            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            this.inputContext = ClientInputContext
                                .Start("Debug tools overlay")
                                .HandleButtonDown(GameButton.CancelOrClose, () => Toggle(isEnabled: false));
        }

        protected override void OnUnloaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.DataContext = null;
            this.overlay.MouseLeftButtonDown -= this.OverlayMouseLeftButtonDownHandler;
            this.inputContext.Stop();
            this.inputContext = null;
        }

        private static void CreateInstance()
        {
            if (instance != null)
            {
                return;
            }

            instance = new DebugToolsOverlay();
            Api.Client.UI.LayoutRootChildren.Add(instance);
            Panel.SetZIndex(instance, 1000);
        }

        private static void DestroyInstance()
        {
            if (instance == null)
            {
                return;
            }

            ((Panel)instance.Parent).Children.Remove(instance);
            instance = null;
        }

        private void OverlayMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            DestroyInstance();
        }
    }
}