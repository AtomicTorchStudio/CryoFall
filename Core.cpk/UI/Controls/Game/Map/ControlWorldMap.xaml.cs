namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ControlWorldMap : BaseUserControl
    {
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

        private PanningPanel panningPanel;

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

        public WorldMapController WorldMapController { get; private set; }

        public void CenterMapOnPlayerCharacter()
        {
            this.WorldMapController?.CenterMapOnPlayerCharacter();
        }

        protected override void InitControl()
        {
            this.panningPanel = this.GetByName<PanningPanel>("PanningPanel");
            var layoutRoot = VisualTreeHelper.GetChild(this.panningPanel, 0) as FrameworkElement;
            var textBlockCurrentPosition = layoutRoot.GetByName<TextBlock>("TextBlockCurrentPosition");
            var textBlockPointedPosition = layoutRoot.GetByName<TextBlock>("TextBlockPointedPosition");

            this.WorldMapController = new WorldMapController(
                this.panningPanel,
                textBlockCurrentPosition,
                textBlockPointedPosition,
                this.IsEditor);

            this.CommandCenter = new ActionCommand(this.CenterMapOnPlayerCharacter);
        }

        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            this.WorldMapController.Dispose();
            this.WorldMapController = null;
        }
    }
}