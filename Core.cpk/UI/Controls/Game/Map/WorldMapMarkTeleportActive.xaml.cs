namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows;
    using System.Windows.Media.Animation;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public partial class WorldMapMarkTeleportActive : BaseUserControl
    {
        public static readonly DependencyProperty IsCurrentTeleportProperty
            = DependencyProperty.Register(nameof(IsCurrentTeleport),
                                          typeof(bool),
                                          typeof(WorldMapMarkTeleportActive),
                                          new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty TeleportTitleProperty =
            DependencyProperty.Register(nameof(TeleportTitle),
                                        typeof(string),
                                        typeof(WorldMapMarkTeleportActive),
                                        new PropertyMetadata(default(string)));

        public bool IsCurrentTeleport
        {
            get => (bool)this.GetValue(IsCurrentTeleportProperty);
            set => this.SetValue(IsCurrentTeleportProperty, value);
        }

        public string TeleportTitle
        {
            get => (string)this.GetValue(TeleportTitleProperty);
            set => this.SetValue(TeleportTitleProperty, value);
        }

        public Vector2Ushort WorldPosition { get; set; }

        protected override void InitControl()
        {
            this.DataContext = this;
        }

        protected override void OnLoaded()
        {
            if (!this.IsCurrentTeleport)
            {
                this.GetResource<Storyboard>("StoryboardAnimation")
                    .Begin(this.GetByName<FrameworkElement>("LayoutRoot"));
            }
        }
    }
}