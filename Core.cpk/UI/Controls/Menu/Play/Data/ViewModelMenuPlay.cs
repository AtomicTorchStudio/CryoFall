namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Play.Data
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Demo;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelMenuPlay : BaseViewModel
    {
        private bool isTabLocalServerSelected;

        private bool isTabMultiplayerSelected;

        public bool IsAnyTabSelected { get; set; }

        public bool IsTabLocalServerSelected
        {
            get => this.isTabLocalServerSelected;
            set
            {
                if (this.isTabLocalServerSelected == value)
                {
                    return;
                }

                if (value
                    && Api.Client.MasterServer.IsDemoVersion)
                {
                    value = false;
                    DemoVersionDialogWindow.ShowDialog();
                }

                this.isTabLocalServerSelected = value;
                this.NotifyThisPropertyChanged();

                if (value)
                {
                    this.IsAnyTabSelected = true;
                }
            }
        }

        public bool IsTabMultiplayerSelected
        {
            get => this.isTabMultiplayerSelected;
            set
            {
                if (this.isTabMultiplayerSelected == value)
                {
                    return;
                }

                this.isTabMultiplayerSelected = value;
                this.NotifyThisPropertyChanged();

                if (value)
                {
                    this.IsAnyTabSelected = true;
                }

                ViewModelMenuServers.Instance?.ResetSortOrder();

                // ensure master server is connected
                Client.MasterServer.Connect();
            }
        }
    }
}