namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.CurrentGame.Data
{
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public static class ClientCurrentGameServerRatesEditorHelper
    {
        public const string DialogNoChangesMade = "No changes were made.";

        public const string DialogServerRebootNecessary
            = "To apply server rates changes, the server needs to save, stop, and reload. The server restart may take a while.";

        public static void OpenEditorWindow()
        {
            var editor = new CurrentServerRatesEditorControl();
            editor.CompactTiles = true;
            editor.Loaded += RatesEditorControlLoadedHandler;

            var dialogWindow = DialogWindow.ShowDialog(
                title: null,
                content: new ScrollViewer()
                {
                    Content = editor,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                    VerticalAlignment = VerticalAlignment.Top
                },
                okAction: OpenChangesConfirmationDialog,
                cancelAction: () => { },
                okText: CoreStrings.Button_Save);

            dialogWindow.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            dialogWindow.VerticalContentAlignment = VerticalAlignment.Top;
            dialogWindow.GameWindow.Width = 600;
            dialogWindow.GameWindow.Height = 600;
            dialogWindow.GameWindow.FocusOnControl = null;

            void OpenChangesConfirmationDialog()
            {
                var rates = editor.RatesEditor.GetRates();
                var serverRatesConfig = Api.Client.Core.CreateNewServerRatesConfig();
                foreach (var rate in rates)
                {
                    rate.Rate.SharedApplyToConfig(serverRatesConfig, rate.GetAbstractValue());
                }

                if (!HasChanges(serverRatesConfig))
                {
                    DialogWindow.ShowMessage(title: DialogNoChangesMade,
                                             text: null,
                                             closeByEscapeKey: true);
                    return;
                }

                DialogWindow.ShowDialog(title: CoreStrings.QuestionAreYouSure,
                                        text: DialogServerRebootNecessary,
                                        closeByEscapeKey: true,
                                        okText: CoreStrings.Button_Apply,
                                        okAction: () =>
                                                  {
                                                      RatesSynchronizationSystem.ClientConfigureRatesOnServer(
                                                          serverRatesConfig);
                                                  },
                                        cancelAction: () => { },
                                        focusOnCancelButton: true);
            }
        }

        private static bool HasChanges(IServerRatesConfig otherRatesConfig)
        {
            var currentRates = RatesManager.Rates.ToDictionary(r => r,
                                                               r => r.SharedAbstractValue);
            var currentRatesConfig = Api.Client.Core.CreateNewServerRatesConfig();
            foreach (var rate in currentRates)
            {
                rate.Key.SharedApplyToConfig(currentRatesConfig, rate.Value);
            }

            return !currentRatesConfig.IsMatches(otherRatesConfig);
        }

        private static void RatesEditorControlLoadedHandler(object sender, RoutedEventArgs e)
        {
            var ratesEditorControl = ((CurrentServerRatesEditorControl)sender).RatesEditor;
            ratesEditorControl.UpdateLayout();
            ratesEditorControl.SetRates(
                RatesManager.Rates.ToDictionary(r => r,
                                                r => r.SharedAbstractValue));
        }
    }
}