namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Demo
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class DemoVersionDialogWindow
    {
        public static void ShowDialog()
        {
            DialogWindow.ShowDialog(
                CoreStrings.Demo_Title,
                CoreStrings.Demo_OnlyOfficialServers,
                okText: CoreStrings.Demo_Button_BuyGameOnSteam,
                okAction: () => Api.Client.ExternalApi.OpenBuyGamePage(),
                cancelText: CoreStrings.Button_Cancel,
                cancelAction: () => { },
                focusOnCancelButton: true,
                closeByEscapeKey: true,
                zIndexOffset: 100000);
        }
    }
}