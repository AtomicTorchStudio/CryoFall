namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System.Windows;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class WorldMapMarkTeleportInactive : BaseUserControl
    {
        public static readonly DependencyProperty TeleportTitleProperty =
            DependencyProperty.Register(nameof(TeleportTitle),
                                        typeof(string),
                                        typeof(WorldMapMarkTeleportInactive),
                                        new PropertyMetadata(default(string)));

        public string TeleportTitle
        {
            get => (string)this.GetValue(TeleportTitleProperty);
            set => this.SetValue(TeleportTitleProperty, value);
        }

        protected override void InitControl()
        {
            base.InitControl();
            this.DataContext = this;
        }
    }
}