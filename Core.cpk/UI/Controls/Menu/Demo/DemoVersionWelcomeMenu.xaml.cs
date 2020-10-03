namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Demo
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Demo.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class DemoVersionWelcomeMenu : BaseUserControl
    {
        private static readonly IClientStorage Storage
            = Api.Client.Storage.GetSessionStorage("IsDemoVersionWelcomeMessageDisplayed");

        private static DemoVersionWelcomeMenu instance;

        public static bool IsDisplayed
        {
            get => instance is not null;
            set
            {
                if (IsDisplayed == value)
                {
                    return;
                }

                if (value)
                {
                    // must be displayed
                    if (instance is not null)
                    {
                        // already displayed
                        return;
                    }

                    // create new instance and add into layout root
                    instance = new DemoVersionWelcomeMenu();
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
            }
        }

        public static bool WasDisplayedDuringThisSession
        {
            get => Storage.TryLoad(out bool result) && result;
            set => Storage.Save(value);
        }

        public static void Close()
        {
            WasDisplayedDuringThisSession = true;
            IsDisplayed = false;
        }

        public static void DisplayIfRequired()
        {
            IsDisplayed = Api.Client.MasterServer.IsDemoVersion
                          && (Api.Client.MasterServer.DemoIsExpired
                              || !WasDisplayedDuringThisSession);
        }

        protected override void OnLoaded()
        {
            this.DataContext = new ViewModelDemoVersionWelcomeMenu();
        }

        protected override void OnUnloaded()
        {
            var viewModel = (ViewModelDemoVersionWelcomeMenu)this.DataContext;
            this.DataContext = null;
            viewModel.Dispose();
        }
    }
}