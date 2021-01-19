namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows;
    using System.Windows.Input;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ControlWorldMap : BaseUserControl
    {
        public const bool DefaultIsFullOpacity = true;

        public static readonly DependencyProperty CommandCenterProperty =
            DependencyProperty.Register(
                nameof(CommandCenter),
                typeof(BaseCommand),
                typeof(ControlWorldMap),
                new PropertyMetadata(default(BaseCommand)));

        public static readonly DependencyProperty IsEditorProperty = DependencyProperty.Register(
            nameof(IsEditor),
            typeof(bool),
            typeof(ControlWorldMap),
            new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsFullOpacityNowProperty =
            DependencyProperty.Register(nameof(IsFullOpacityNow),
                                        typeof(bool),
                                        typeof(ControlWorldMap),
                                        new PropertyMetadata(DefaultIsFullOpacity));

        private PanningPanel panningPanel;

        private ViewModelControlWorldMap viewModel;

        public BaseCommand CommandCenter
        {
            get => (BaseCommand)this.GetValue(CommandCenterProperty);
            set => this.SetValue(CommandCenterProperty, value);
        }

        public bool IsEditor
        {
            get => (bool)this.GetValue(IsEditorProperty);
            set => this.SetValue(IsEditorProperty, value);
        }

        public bool IsFullOpacityNow
        {
            get => (bool)this.GetValue(IsFullOpacityNowProperty);
            set => this.SetValue(IsFullOpacityNowProperty, value);
        }

        public WorldMapController WorldMapController { get; private set; }

        public void CenterMapOnPlayerCharacter()
        {
            this.WorldMapController?.CenterMapOnPlayerCharacter(resetZoomIfBelowThreshold: true);
        }

        protected override void InitControl()
        {
            this.panningPanel = this.GetByName<PanningPanel>("PanningPanel");
            this.DataContext = this.viewModel = new ViewModelControlWorldMap();
            this.CommandCenter = new ActionCommand(this.CenterMapOnPlayerCharacter);
        }

        protected override void OnLoaded()
        {
            this.WorldMapController?.Dispose();
            this.WorldMapController = new WorldMapController(
                this.panningPanel,
                sectorProvider: WorldMapSectorProviderHelper.GetProvider(isEditor: this.IsEditor),
                viewModelControlWorldMap: this.viewModel,
                isPlayerMarkDisplayed: !this.IsEditor,
                isCurrentCameraViewDisplayed: this.IsEditor,
                isListeningToInput: true,
                paddingChunks: this.IsEditor ? 10 : 0);

            this.MouseDown += this.MouseDownHandler;
            this.IsFullOpacityNow = this.IsEditor
                                    || DefaultIsFullOpacity;
        }

        protected override void OnUnloaded()
        {
            this.WorldMapController.Dispose();
            this.WorldMapController = null;

            this.MouseDown -= this.MouseDownHandler;
        }

        private void MouseDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (this.IsEditor
                || DefaultIsFullOpacity)
            {
                return;
            }

            this.IsFullOpacityNow = true;
            Api.Client.UI.LayoutRoot.MouseUp += this.MouseUpHandler;
        }

        private void MouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (this.IsEditor
                || DefaultIsFullOpacity)
            {
                return;
            }

            this.IsFullOpacityNow = false;
            Api.Client.UI.LayoutRoot.MouseUp -= this.MouseUpHandler;
        }
    }
}