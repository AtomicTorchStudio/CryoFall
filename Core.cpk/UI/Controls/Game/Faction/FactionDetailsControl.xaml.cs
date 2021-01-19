namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Faction.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class FactionDetailsControl : BaseUserControl
    {
        private readonly string clanTag;

        private DialogWindow dialogWindow;

        private FactionListEntry factionEntry;

        private ViewModelFactionEntry viewModel;

        private FactionDetailsControl(string clanTag)
        {
            this.clanTag = clanTag;
        }

        public static void Show(string clanTag)
        {
            var control = new FactionDetailsControl(clanTag);
            var dialogWindow = DialogWindow.ShowDialog(
                title: null,
                control,
                okText: CoreStrings.Button_Close,
                closeByEscapeKey: true);
            dialogWindow.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            dialogWindow.VerticalContentAlignment = VerticalAlignment.Stretch;
            dialogWindow.HorizontalAlignment = HorizontalAlignment.Center;
            dialogWindow.VerticalAlignment = VerticalAlignment.Center;
            dialogWindow.GameWindow.Padding = new Thickness(6, 8, 6, 8);
            dialogWindow.GameWindow.Width = Api.Client.UI.GetApplicationResource<float>("GameMenuStandardWindowWidth");
            dialogWindow.GameWindow.RefreshWindowSize();
            control.dialogWindow = dialogWindow;
        }

        protected override void InitControl()
        {
            base.InitControl();
            this.RequestData();
        }

        protected override void OnLoaded()
        {
            this.Refresh();
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }

        private void Refresh()
        {
            if (!this.isLoaded
                || string.IsNullOrEmpty(this.factionEntry.ClanTag))
            {
                return;
            }

            if (this.viewModel != null)
            {
                this.DataContext = null;
                this.viewModel.Dispose();
            }

            this.viewModel = new ViewModelFactionEntry(this.factionEntry,
                                                       isLeaderboardEntry: false,
                                                       isPreviewEntry: false);
            this.DataContext = this.viewModel;
            this.dialogWindow.GameWindow.Height = double.NaN;
            this.dialogWindow.GameWindow.RefreshWindowSize();
        }

        private async void RequestData()
        {
            var entry = await FactionSystem.ClientGetFactionEntry(this.clanTag);
            if (this.IsDisposed)
            {
                // received too late
                return;
            }

            this.factionEntry = entry;
            this.Refresh();
        }
    }
}