namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.UI.Services;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class GameWindow : BaseContentControl
    {
        public static readonly DependencyProperty CloseByEscapeKeyProperty =
            DependencyProperty.Register(
                nameof(CloseByEscapeKey),
                typeof(bool),
                typeof(GameWindow),
                new PropertyMetadata(true));

        public static readonly DependencyProperty FocusOnControlProperty =
            DependencyProperty.Register(
                nameof(FocusOnControl),
                typeof(FrameworkElement),
                typeof(GameWindow),
                new PropertyMetadata(default(FrameworkElement)));

        public static readonly DependencyProperty SoundClosingProperty =
            DependencyProperty.Register(
                nameof(SoundClosing),
                typeof(SoundUI),
                typeof(GameWindow),
                new PropertyMetadata(default(SoundUI)));

        public static readonly DependencyProperty SoundOpeningProperty =
            DependencyProperty.Register(
                nameof(SoundOpening),
                typeof(SoundUI),
                typeof(GameWindow),
                new PropertyMetadata(default(SoundUI)));

        public static readonly DependencyProperty ZIndexOffsetProperty = DependencyProperty.Register(
            nameof(ZIndexOffset),
            typeof(int),
            typeof(GameWindow),
            new PropertyMetadata(default(int)));

        private static readonly IUIClientService UI = Api.IsClient ? Api.Client.UI : null;

        internal ClientInputContext CloseByEscapeKeyInputContext;

        private double calculatedWindowHeight;

        private double calculatedWindowWidth;

        private Grid layoutRoot;

        private GameWindowState state;

        private Storyboard storyboardClose;

        private Storyboard storyboardOpen;

        private FrameworkElement windowChrome;

        static GameWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(GameWindow),
                new FrameworkPropertyMetadata(typeof(GameWindow)));
        }

        public event Action<GameWindow> StateChanged;

        public bool CloseByEscapeKey
        {
            get => (bool)this.GetValue(CloseByEscapeKeyProperty);
            set => this.SetValue(CloseByEscapeKeyProperty, value);
        }

        public BaseCommand CommandCloseCancel
            => new ActionCommand(() => this.Close(DialogResult.Cancel));

        public BaseCommand CommandCloseOk
            => new ActionCommand(() => this.Close(DialogResult.OK));

        public int CurrentZIndex { get; set; }

        public DialogResult DialogResult { get; private set; }

        public FrameworkElement FocusOnControl
        {
            get => (FrameworkElement)this.GetValue(FocusOnControlProperty);
            set => this.SetValue(FocusOnControlProperty, value);
        }

        public bool IsCached { get; set; }

        /// <summary>
        /// Please do not modify this property!
        /// </summary>
        public bool IsDestroyed { get; set; }

        public DependencyObject LinkedParent { get; set; }

        public SoundUI SoundClosing
        {
            get => (SoundUI)this.GetValue(SoundClosingProperty);
            set => this.SetValue(SoundClosingProperty, value);
        }

        public SoundUI SoundOpening
        {
            get => (SoundUI)this.GetValue(SoundOpeningProperty);
            set => this.SetValue(SoundOpeningProperty, value);
        }

        public GameWindowState State
        {
            get => this.state;
            private set
            {
                if (this.state == value)
                {
                    return;
                }

                this.state = value;
                switch (this.state)
                {
                    case GameWindowState.Opening:
                        this.windowChrome.Visibility = Visibility.Visible;
                        SoundUI.PlaySound(this.SoundOpening);
                        break;

                    case GameWindowState.Closing:
                        SoundUI.PlaySound(this.SoundClosing);
                        break;

                    case GameWindowState.Closed:
                        this.windowChrome.Visibility = Visibility.Collapsed;
                        WindowsManager.UnregisterWindow(this);
                        break;
                }

                WindowsManager.OnWindowStateChanged();
                this.StateChanged?.Invoke(this);
            }
        }

        public string WindowName { get; set; }

        public int ZIndexOffset
        {
            get => (int)this.GetValue(ZIndexOffsetProperty);
            set => this.SetValue(ZIndexOffsetProperty, value);
        }

        public void AddExtensionControl(Control controlToInject)
        {
            var firstChild = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            var panel = firstChild.FindName<Panel>("ExtensionsPanel");

            if (panel.Children.Count > 0)
            {
                // not the first element, so add a margin
                controlToInject.Margin = new Thickness(0, 6, 0, 0);
            }

            controlToInject.HorizontalAlignment = HorizontalAlignment.Left;
            panel.Children.Add(controlToInject);
        }

        public void Close(DialogResult dialogResult)
        {
            if (IsDesignTime)
            {
                return;
            }

            switch (this.state)
            {
                case GameWindowState.Closed:
                case GameWindowState.Closing:
                    return;

                case GameWindowState.Opening:
                    //InputBlocker.Unblock();
                    this.storyboardOpen.Stop(this.windowChrome);
                    break;
            }

            //InputBlocker.Block();
            this.DialogResult = dialogResult;

            Api.Logger.Info("Window closing: " + this.WindowName);
            this.State = GameWindowState.Closing;

            this.storyboardClose.Begin(this.windowChrome);
            Cleanup();
        }

        public void Open()
        {
            if (IsDesignTime)
            {
                this.storyboardOpen?.Begin(this.windowChrome);
                return;
            }

            if (this.IsDestroyed)
            {
                throw new Exception("Window is destroyed: " + this);
            }

            switch (this.state)
            {
                case GameWindowState.Opened:
                case GameWindowState.Opening:
                    // already opened or opening
                    return;

                case GameWindowState.Closing:
                    //InputBlocker.Unblock();
                    this.storyboardClose.Stop(this.windowChrome);
                    break;
            }

            this.RefreshWindowSize();

            //InputBlocker.Block();
            this.storyboardOpen.Begin(this.windowChrome);
            Api.Logger.Info("Window opening: " + this.WindowName);
            this.State = GameWindowState.Opening;

            if (this.state == GameWindowState.Opening
                || this.state == GameWindowState.Closing)
            {
                Cleanup();
            }

            WindowsManager.RegisterWindow(this);
        }

        public void RefreshWindowSize()
        {
            if (!this.isLoaded)
            {
                return;
            }

            this.CalcAndSetWindowSize();
            // binding don't work! so set values from here
            this.Width = this.calculatedWindowWidth;
            this.Height = this.calculatedWindowHeight;
        }

        public void Toggle()
        {
            if (this.state
                    is GameWindowState.Opened 
                    or GameWindowState.Opening)
            {
                this.Close(DialogResult.Cancel);
            }
            else
            {
                this.Open();
            }
        }

        public override string ToString()
        {
            return "GameWindow: " + this.WindowName;
        }

        protected override void InitControl()
        {
            var templateRoot = (FrameworkElement)VisualTreeHelper.GetChild(this, 0);
            this.windowChrome = templateRoot.GetByName<FrameworkElement>("WindowChrome");
            this.layoutRoot = templateRoot.GetByName<Grid>("LayoutRoot");
            this.storyboardOpen = (Storyboard)this.Template.Resources["StoryboardOpen"];
            this.storyboardClose = (Storyboard)this.Template.Resources["StoryboardClose"];

            this.WindowName = this.Parent?.GetType().Name ?? "noname";

            if (!IsDesignTime)
            {
                // initial chrome state - collapsed
                this.windowChrome.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnLoaded()
        {
            this.RefreshWindowSize();

            if (IsDesignTime)
            {
                this.Open();
                return;
            }

            this.storyboardOpen.Completed += this.StoryboardOpenCompletedHandler;
            this.storyboardClose.Completed += this.StoryboardCloseCompletedHandler;
        }

        protected override void OnUnloaded()
        {
            if (IsDesignTime)
            {
                return;
            }

            this.storyboardOpen.Completed -= this.StoryboardOpenCompletedHandler;
            this.storyboardClose.Completed -= this.StoryboardCloseCompletedHandler;

            // ensure the window is unregistered
            WindowsManager.UnregisterWindow(this);
        }

        private static void Cleanup()
        {
            UI.BlurFocus();
            PopupsManagerService.CloseAllOpenedComboboxes();
            ToolTipServiceExtend.CloseOpenedTooltip();
        }

        private void CalcAndSetWindowSize()
        {
            double availableWidth = UI.TargetUIWidth,
                   availableHeight = UI.TargetUIHeight,
                   definedWidth = this.Width,
                   definedHeight = this.Height;

            bool useDefinedWidthProperty = false,
                 useDefinedHeightProperty = false;

            if (!double.IsNaN(definedWidth)
                && definedWidth > 0)
            {
                availableWidth = definedWidth;
                useDefinedWidthProperty = true;
            }

            if (!double.IsNaN(definedHeight)
                && definedHeight > 0)
            {
                availableHeight = definedHeight;
                useDefinedHeightProperty = true;
            }

            var desiredSize = Size.Empty;
            if (!useDefinedWidthProperty
                || !useDefinedHeightProperty)
            {
                // width or height is not defined - calculate desired size (in available size)
                this.layoutRoot.UpdateLayout();
                this.layoutRoot.Measure(availableSize: new Size(availableWidth, availableHeight));
                desiredSize = this.layoutRoot.DesiredSize;
            }

            this.calculatedWindowWidth = useDefinedWidthProperty ? definedWidth : desiredSize.Width;
            this.calculatedWindowHeight = useDefinedHeightProperty ? definedHeight : desiredSize.Height;
        }

        private void StoryboardCloseCompletedHandler(object sender, EventArgs e)
        {
            this.State = GameWindowState.Closed;
            this.IsEnabled = false;

            Api.Logger.Info("Window closed: " + this.WindowName);
        }

        private void StoryboardOpenCompletedHandler(object sender, EventArgs e)
        {
            this.State = GameWindowState.Opened;
            this.IsEnabled = true;

            //InputBlocker.Unblock();
            (this.FocusOnControl ?? this.layoutRoot).Focus();

            Api.Logger.Info("Window opened: " + this.WindowName);
        }
    }
}