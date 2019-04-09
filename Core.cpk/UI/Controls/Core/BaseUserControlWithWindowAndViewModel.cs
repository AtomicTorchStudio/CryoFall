namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using AtomicTorch.CBND.GameApi.Scripting;

    public class BaseUserControlWithWindowAndViewModel<TControl, TViewModel> : BaseUserControlWithWindow
        where TControl : BaseUserControlWithWindowAndViewModel<TControl, TViewModel>, new()
        where TViewModel : BaseViewModel
    {
        private TViewModel viewModel;

        public static TControl Instance { get; private set; }

        public TViewModel ViewModel
        {
            get => this.viewModel;
            private set
            {
                if (this.viewModel == value)
                {
                    return;
                }

                if (this.viewModel != null)
                {
                    this.DataContext = null;
                    this.viewModel.Dispose();
                }

                this.viewModel = value;

                if (this.viewModel != null)
                {
                    this.DataContext = this.viewModel;
                }
            }
        }

        public static TControl Open(TViewModel viewModel)
        {
            if (Instance == null)
            {
                var instance = new TControl();
                instance.ViewModel = viewModel;
                Instance = instance;
                Api.Client.UI.LayoutRootChildren.Add(instance);
            }
            else
            {
                Instance.ViewModel = viewModel;
            }

            return Instance;
        }

        protected override void OnUnloaded()
        {
            this.ViewModel = null;
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}