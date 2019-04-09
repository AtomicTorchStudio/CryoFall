namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.DataLogs
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.DataLogs.Data;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public partial class WindowDataLog : BaseUserControlWithWindow
    {
        private IItem itemDataLog;

        private ViewModelWindowDataLog viewModel;

        public static void Open(IItem itemDataLog)
        {
            var window = new WindowDataLog();
            window.itemDataLog = itemDataLog;
            Api.Client.UI.LayoutRootChildren.Add(window);
        }

        protected override void OnLoaded()
        {
            this.DataContext = this.viewModel = new ViewModelWindowDataLog(this.itemDataLog);
        }

        protected override void OnUnloaded()
        {
            this.DataContext = null;
            this.viewModel.Dispose();
            this.viewModel = null;
        }
    }
}