namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.LocalGame.Data
{
    using System;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Systems.ServerWelcomeMessage;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public readonly struct DataEntryLocalServerSaveGame
    {
        public DataEntryLocalServerSaveGame(LocalServerSavegameEntry entry)
        {
            this.SlotId = entry.SlotId;
            this.Name = entry.Name;
            this.Date = entry.Date;
        }

        public BaseCommand CommandLoad
            => new ActionCommand(this.ExecuteCommandLoad);

        public DateTime Date { get; }

        public string DateText => WelcomeMessageSystem.FormatDate(this.Date);

        public string FileName => "Save" + this.SlotId + ".save";

        public string Name { get; }

        public ushort SlotId { get; }

        private void ExecuteCommandLoad()
        {
            if (this.Name.Contains("[CORRUPTED SAVEGAME]"))
            {
                DialogWindow.ShowDialog(
                    CoreStrings.DialogCannotLoadSavegame_Title,
                    CoreStrings.DialogCannotLoadSavegame_MessageCorrupted_Detected
                    + "[br][br]"
                    + CoreStrings.DialogCannotLoadSavegame_MessageCorrupted_PossibleCauses
                    + "[br][br]"
                    + CoreStrings.DialogCannotLoadSavegame_MessageCorrupted_CanTryRollback
                    + "[br]"
                    + CoreStrings.DialogCannotLoadSavegame_MessageCorrupted_NavigateToFolderWithBackups
                    + " [b]"
                    + this.FileName
                    + "[/b]",
                    closeByEscapeKey: true,
                    textAlignment: TextAlignment.Left,
                    okAction: () => Api.Client.LocalServer.BrowseSavesFolder());
                return;
            }

            Api.Logger.Important("Load savegame: " + this.Name + " SlotID=" + this.SlotId);
            Api.Client.LocalServer.Load(this.SlotId);
        }
    }
}