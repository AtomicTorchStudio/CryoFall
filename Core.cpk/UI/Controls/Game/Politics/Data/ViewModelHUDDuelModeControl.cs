namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Politics.Data
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelHUDDuelModeControl : BaseViewModel
    {
        public ViewModelHUDDuelModeControl()
        {
            ClientCurrentCharacterHelper.PublicState.ClientSubscribe(
                _ => _.IsPveDuelModeEnabled,
                _ => this.NotifyPropertyChanged(nameof(this.Visibility)),
                this);
        }

        public Visibility Visibility => PveSystem.ClientIsDuelModeEnabled
                                            ? Visibility.Visible
                                            : Visibility.Collapsed;
    }
}