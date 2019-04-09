namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class CompilationSplashScreenFailedCompilation : BaseUserControl
    {
        private static ClientInputContext inputContextInterceptor;

        private static bool isDisplayed;

        private static CompilationSplashScreenFailedCompilation overlayControlInstance;

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
                                              .Start("Failed compilation screen - intercept all input")
                                              .HandleAll(ClientInputManager.ConsumeAllButtons);

                    overlayControlInstance = new CompilationSplashScreenFailedCompilation();

                    var viewModel = new ViewModelCompilationSplashScreenFailedCompilation()
                    {
                        CompilationMessagesTest = Api.Client.Core.LastCompilationMessagesText
                    };

                    overlayControlInstance.DataContext = viewModel;
                    Api.Client.UI.LayoutRootChildren.Add(overlayControlInstance);
                }
                else
                {
                    inputContextInterceptor.Stop();
                    inputContextInterceptor = null;

                    Api.Client.UI.LayoutRootChildren.Remove(overlayControlInstance);

                    var viewModel = overlayControlInstance.DataContext as BaseViewModel;
                    overlayControlInstance.DataContext = null;
                    viewModel.Dispose();
                    overlayControlInstance = null;
                }
            }
        }

        protected override void InitControl()
        {
        }
    }
}