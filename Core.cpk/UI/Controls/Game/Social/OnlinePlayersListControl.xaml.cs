namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Social.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class OnlinePlayersListControl : BaseUserControl
    {
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel),
                                        typeof(ViewModelOnlinePlayersList),
                                        typeof(OnlinePlayersListControl),
                                        new PropertyMetadata(default(ViewModelOnlinePlayersList)));

        public ViewModelOnlinePlayersList ViewModel
        {
            get => (ViewModelOnlinePlayersList)this.GetValue(ViewModelProperty);
            set => this.SetValue(ViewModelProperty, value);
        }

        protected override void InitControl()
        {
            this.DataContext = this.ViewModel = new ViewModelOnlinePlayersList() { IsActive = true };
        }
    }
}