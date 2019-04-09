namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CompilationSplashScreen : BaseUserControl
    {
        private static ClientInputContext inputContextInterceptor;

        private static bool isDisplayed;

        private static CompilationSplashScreen overlayControlInstance;

        public static bool IsDisplayed
        {
            get => isDisplayed;
            set
            {
                if (isDisplayed == value)
                {
                    return;
                }

                isDisplayed = value;
                if (isDisplayed)
                {
                    // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                    inputContextInterceptor = ClientInputContext
                                              .Start("Compilation splash screen - intercept all other input)")
                                              .HandleAll(ClientInputManager.ConsumeAllButtons);

                    overlayControlInstance = new CompilationSplashScreen();
                    Api.Client.UI.LayoutRootChildren.Add(overlayControlInstance);
                }
                else
                {
                    inputContextInterceptor.Stop();
                    inputContextInterceptor = null;

                    Api.Client.UI.LayoutRootChildren.Remove(overlayControlInstance);
                    overlayControlInstance = null;
                }
            }
        }

        protected override void InitControl()
        {
        }
    }
}