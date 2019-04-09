namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data
{
    public class ViewModelCompilationSplashScreenFailedCompilation : BaseViewModel
    {
        public ViewModelCompilationSplashScreenFailedCompilation(string compilationMessagesTest)
        {
            this.CompilationMessagesTest = compilationMessagesTest;
        }

        public ViewModelCompilationSplashScreenFailedCompilation()
        {
        }

        public string CompilationMessagesTest { get; set; }
    }
}