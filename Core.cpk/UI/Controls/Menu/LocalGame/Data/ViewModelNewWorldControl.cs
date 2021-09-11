namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.LocalGame.Data
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelNewWorldControl : BaseViewModel
    {
        public BaseCommand CommandToggleRatesEditor
            => new ActionCommand(this.ExecuteCommandToggleRatesEditor);

        public bool IsRatesEditorVisible { get; set; }

        public string SaveName { get; set; } = CoreStrings.NewWorldControl_WorldName_Default;

        public void StartNewGame(string name, IReadOnlyList<IViewModelRate> rates)
        {
            Logger.Important("Creating new world");
            var serverRatesConfig = Client.Core.CreateNewServerRatesConfig();
            foreach (var rate in rates)
            {
                rate.Rate.SharedApplyToConfig(serverRatesConfig, rate.GetAbstractValue());
            }

            name = name?.Trim() ?? string.Empty;
            if (name.Length is 0 or > 30)
            {
                throw new Exception("The name length must be 1-30 characters long");
            }

            Client.LocalServer.CreateNewWorld(slotId: Client.LocalServer.GetNextFreeSlotId(),
                                              name,
                                              Array.Empty<string>(),
                                              serverRatesConfig);
        }

        private void ExecuteCommandToggleRatesEditor()
        {
            this.IsRatesEditorVisible = !this.IsRatesEditorVisible;
        }
    }
}