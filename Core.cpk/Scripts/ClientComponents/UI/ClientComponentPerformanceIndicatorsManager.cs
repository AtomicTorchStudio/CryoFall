namespace AtomicTorch.CBND.CoreMod.ClientComponents.UI
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Performance;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class ClientComponentPerformanceIndicatorsManager : ClientComponent
    {
        private static readonly IClientStorage SessionStorage;

        private static bool isDisplayed;

        private HUDPerformanceIndicatorsPanel panel;

        static ClientComponentPerformanceIndicatorsManager()
        {
            SessionStorage = Api.Client.Storage.GetSessionStorage(
                $"{nameof(HUDPerformanceIndicatorsPanel)}.{nameof(IsDisplayed)}");
            if (!SessionStorage.TryLoad(out isDisplayed))
            {
                // overlay is visible by default
                isDisplayed = true;
            }
        }

        public static event Action IsDisplayedChanged;

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

                SessionStorage.Save(isDisplayed);
                IsDisplayedChanged?.Invoke();
            }
        }

        public override void Update(double deltaTime)
        {
            this.panel.Visibility = isDisplayed ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override void OnEnable()
        {
            this.panel = new HUDPerformanceIndicatorsPanel();
            Api.Client.UI.LayoutRootChildren.Add(this.panel);
        }
    }
}