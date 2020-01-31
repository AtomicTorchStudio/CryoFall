namespace AtomicTorch.CBND.CoreMod.Bootstrappers
{
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Extensions;

    [PrepareOrder(afterType: typeof(BootstrapperClientCoreUI))]
    public class BootstrapperClientDisabledModsNotifier : BaseBootstrapper
    {
        public override void ClientInitialize()
        {
            var disabledMods = Client.Core.DisabledMods;
            if (disabledMods.Length == 0)
            {
                return;
            }

            DialogWindow.ShowDialog(
                CoreStrings.ModsDisabledWindow_Title,
                string.Format(CoreStrings.ModsDisabledWindow_Description.Replace("[br]{0}", "{0}"),
                              disabledMods.Select(s => $"[*][b]{s.Id}[/b] v{s.Version} - {s.Title}")
                                          .GetJoinedString(string.Empty)),
                textAlignment: TextAlignment.Left,
                closeByEscapeKey: false,
                okAction: () => Client.Core.ResetDisabledModsList(),
                zIndexOffset: 9001).Window.FocusOnControl = null;
        }
    }
}