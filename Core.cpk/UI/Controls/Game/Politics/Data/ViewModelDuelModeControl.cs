namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelDuelModeControl : BaseViewModel
    {
        public ViewModelDuelModeControl()
        {
            PveSystem.ClientIsPvEChanged += this.RefreshAll;
            ClientCurrentCharacterHelper.PublicState.ClientSubscribe(
                _ => _.IsPveDuelModeEnabled,
                _ => this.RefreshAll(),
                this);
        }

        public BaseCommand CommandToggleDuelMode
            => new ActionCommand(this.ExecuteCommandToggleDuelMode);

        public bool IsDuelModeEnabled
        {
            get => PveSystem.ClientIsDuelModeEnabled;
            set => PveSystem.ClientIsDuelModeEnabled = value;
        }

        public Visibility Visibility
            => PveSystem.ClientIsPve(logErrorIfDataIsNotYetAvailable: false)
                   ? Visibility.Visible
                   : Visibility.Collapsed;

        private void ExecuteCommandToggleDuelMode()
        {
            this.IsDuelModeEnabled = !this.IsDuelModeEnabled;
        }

        private void RefreshAll()
        {
            this.NotifyPropertyChanged(nameof(this.Visibility));
            this.NotifyPropertyChanged(nameof(this.IsDuelModeEnabled));
        }
    }
}