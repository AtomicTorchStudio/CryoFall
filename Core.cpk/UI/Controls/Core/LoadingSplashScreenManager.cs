namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterCreation;
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
            = new(true);

        private TaskCompletionSource<bool> ticketIsShown
            = new(true);

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

                if (value == LoadingSplashScreenState.Shown)
                {
                    // showing completed
                    if (!this.ticketIsShown.Task.IsCompleted)
                    {
                        this.ticketIsShown.SetResult(true);
                    }
                }
                else
                {
                    // not shown anymore - create new ticket if required
                    if (this.ticketIsShown.Task.IsCompleted)
                    {
                        this.ticketIsShown = new TaskCompletionSource<bool>();
                    }
                }
            }
        }

        public static void Hide()
        {
            Instance.Hide();
        }

        public static void Show(string reason, bool displayStructureInfos = true)
        {
            Instance.Show(reason, displayStructureInfos);
        }

        public static Task WaitHiddenAsync()
        {
            return instance.ticketIsHidden.Task;
        }

        public static Task WaitShownAsync()
        {
            return instance.ticketIsShown.Task;
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
                || this.CurrentState == LoadingSplashScreenState.Hiding
                || this.CurrentState == LoadingSplashScreenState.Showing)
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
        }

        void ILoadingSplashScreenManager.Show(string reason, bool displayStructureInfos)
        {
            isCanHide = false;

            if (this.CurrentState == LoadingSplashScreenState.Shown
                || this.CurrentState == LoadingSplashScreenState.Showing)
            {
                Api.Logger.Info(
                    "Loading splash screen already showing, requested to show it again because: " + reason);
                return;
            }

            Api.Logger.Info(
                $"Loading splash screen start showing: {reason} (display structure info: {displayStructureInfos})");
            this.CurrentState = LoadingSplashScreenState.Showing;
            this.instanceControl.Visibility = Visibility.Visible;
            this.instanceControl.DisplayStructureInfos = displayStructureInfos;
            this.instanceControl.Refresh();
        }

        private static bool CheckIsCanHideLoadingScreen()
        {
            if (!Client.Scene.IsEverythingLoaded)
            {
                // some assets are not loaded yet
                return false;
            }

            if (Client.CurrentGame.ConnectionState == ConnectionState.Connected)
            {
                if (!TimeOfDaySystem.ClientIsInitialized)
                {
                    // not yet received the current time from the server
                    return false;
                }

                if (CharacterCreationSystem.SharedIsEnabled)
                {
                    if (Api.Client.World.AvailableWorldChunksCount == 0
                        && ClientCurrentCharacterHelper.Character is not null
                        && CharacterCreationSystem.SharedIsCharacterCreated(ClientCurrentCharacterHelper.Character))
                    {
                        // character is created but some world chunks are not yet loaded
                        return false;
                    }
                }
            }

            return true;
        }
    }
}