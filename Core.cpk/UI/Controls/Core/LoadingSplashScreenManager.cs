namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class LoadingSplashScreenManager : ClientComponent, ILoadingSplashScreenManager
    {
        private const byte DelayAfterEverythingIsLoadedFrames = 5;

        private static int framesRemainsAfterEverythingIsLoaded;

        private static LoadingSplashScreenManager instance;

        private static bool isCanHide;

        private readonly LoadingSplashScreen instanceControl;

        private LoadingSplashScreenState currentState = LoadingSplashScreenState.Hidden;

        private TaskCompletionSource<bool> ticketIsHidden
            = new TaskCompletionSource<bool>(true);

        public LoadingSplashScreenManager()
        {
            if (instance is not null)
            {
                throw new Exception("Instance already created");
            }

            this.instanceControl = new LoadingSplashScreen();
            this.instanceControl.HideAnimationCompleted += this.OnHideAnimationCompleted;
            this.instanceControl.ShowAnimationCompleted += this.OnShowAnimationCompleted;
            Api.Client.UI.LayoutRootChildren.Add(this.instanceControl);
        }

        public event Action StateChanged;

        public static ILoadingSplashScreenManager Instance
        {
            get
            {
                if (instance is null)
                {
                    instance = Api.Client.Scene.CreateSceneObject("Loading splash screen governor")
                                  .AddComponent<LoadingSplashScreenManager>();
                }

                return instance;
            }
        }

        public LoadingSplashScreenState CurrentState
        {
            get => this.currentState;
            private set
            {
                if (this.currentState == value)
                {
                    return;
                }

                this.currentState = value;
                this.StateChanged?.Invoke();

                if (value == LoadingSplashScreenState.Hidden)
                {
                    // hidden completed
                    if (!this.ticketIsHidden.Task.IsCompleted)
                    {
                        this.ticketIsHidden.SetResult(true);
                    }
                }
                else
                {
                    // not hidden anymore - create new ticket if required
                    if (this.ticketIsHidden.Task.IsCompleted)
                    {
                        this.ticketIsHidden = new TaskCompletionSource<bool>();
                    }
                }
            }
        }

        public static void Hide()
        {
            Instance.Hide();
        }

        public static void Show(string reason)
        {
            Instance.Show(reason);
        }

        public static Task WaitHiddenAsync()
        {
            return instance.ticketIsHidden.Task;
        }

        public void OnHideAnimationCompleted()
        {
            Api.Logger.Info("Loading splash screen hidden");
            //Api.Client.UI.LayoutRootChildren.Remove(this.instanceControl);
            this.CurrentState = LoadingSplashScreenState.Hidden;
            this.instanceControl.Visibility = Visibility.Collapsed;
        }

        public void OnShowAnimationCompleted()
        {
            Api.Logger.Info("Loading splash screen shown");
            this.CurrentState = LoadingSplashScreenState.Shown;
        }

        public override void Update(double deltaTime)
        {
            if (!isCanHide
                || this.CurrentState == LoadingSplashScreenState.Hidden
                || this.CurrentState == LoadingSplashScreenState.Hiding)
            {
                return;
            }

            // check if can hide
            if (!CheckIsCanHideLoadingScreen())
            {
                // some assets not yet loaded - setup delay since now
                framesRemainsAfterEverythingIsLoaded = DelayAfterEverythingIsLoadedFrames;
                return;
            }

            framesRemainsAfterEverythingIsLoaded--;
            if (framesRemainsAfterEverythingIsLoaded > 0)
            {
                // delay in progress
                return;
            }

            // delay completed - start hiding splash screen
            Api.Logger.Info("Loading splash screen go to hidden state");
            this.CurrentState = LoadingSplashScreenState.Hiding;
            this.instanceControl.Refresh();
        }

        void ILoadingSplashScreenManager.Hide()
        {
            if (this.CurrentState == LoadingSplashScreenState.Hiding
                || this.CurrentState == LoadingSplashScreenState.Hidden
                || isCanHide)
            {
                return;
            }

            isCanHide = true;
            Api.Logger.Info("Loading splash screen allowed to be hidden - awaiting while everything is loaded");
            framesRemainsAfterEverythingIsLoaded = DelayAfterEverythingIsLoadedFrames;
            //this.CurrentState = LoadingSplashScreenState.Hiding;
            //this.instanceControl.Refresh();
        }

        void ILoadingSplashScreenManager.Show(string reason)
        {
            if (this.CurrentState == LoadingSplashScreenState.Shown
                || this.CurrentState == LoadingSplashScreenState.Showing)
            {
                return;
            }

            Api.Logger.Info("Loading splash screen start showing: " + reason);
            this.CurrentState = LoadingSplashScreenState.Showing;
            this.instanceControl.Visibility = Visibility.Visible;
            this.instanceControl.Refresh();
        }

        private static bool CheckIsCanHideLoadingScreen()
        {
            if (!Client.Scene.IsEverythingLoaded)
            {
                // some assets are not loaded yet
                return false;
            }

            if (Client.CurrentGame.ConnectionState == ConnectionState.Connected
                && !TimeOfDaySystem.ClientIsInitialized)
            {
                // not yet received the current time from the server
                return false;
            }

            return true;
        }
    }
}