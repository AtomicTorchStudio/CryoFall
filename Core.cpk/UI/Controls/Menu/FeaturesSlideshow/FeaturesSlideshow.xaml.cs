namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.FeaturesSlideshow
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.FeaturesSlideshow.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FeaturesSlideshow : BaseUserControl
    {
        private static readonly IClientStorage Storage
            = Api.Client.Storage.GetStorage("FeaturesSlideshowFinished-A29");

        private static FeaturesSlideshow instance;

        private static ViewModelFeaturesSlideshow viewModel;

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
                    viewModel = new ViewModelFeaturesSlideshow();
                    instance = new FeaturesSlideshow();
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
        }

        public static bool IsSlideShowFinishedAtLeastOnce
        {
            get => Api.IsEditor
                   || Api.Shared.IsDebug
                   || Storage.TryLoad(out bool result) && result;
            set => Storage.Save(value);
        }

        public static void DisplayIfRequired()
        {
            IsDisplayed = !IsSlideShowFinishedAtLeastOnce;
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