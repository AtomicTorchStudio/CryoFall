namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class ClientConsoleCommandsHistory
    {
        private const int MaxEntries = 50;

        private static readonly IClientStorage Storage;

        private readonly List<string> entries;

        private int currentEntryIndex;

        private string notCommittedCommand = string.Empty;

        static ClientConsoleCommandsHistory()
        {
            Storage = Api.Client.Storage.GetStorage("ConsoleCommandsHistory");
        }

        public ClientConsoleCommandsHistory()
        {
            this.entries = this.TryLoad();
        }

        public void AddEntry(string command)
        {
            if (this.entries.LastOrDefault() == command)
            {
                // last entry is already the same, don't add duplicate
            }
            else
            {
                this.entries.Add(command);

                if (this.entries.Count > MaxEntries)
                {
                    this.entries.RemoveRange(0, this.entries.Count - MaxEntries);
                }

                this.Save();
            }

            this.notCommittedCommand = string.Empty;
            this.currentEntryIndex = this.entries.Count;
        }

        public string TryGetNextEntry(string currentInput)
        {
            if (this.currentEntryIndex >= this.entries.Count)
            {
                // cannot go forward
                return currentInput;
            }

            this.currentEntryIndex++;
            if (this.currentEntryIndex < this.entries.Count)
            {
                // return history entry
                return this.entries[this.currentEntryIndex];
            }

            // return not commited command
            return this.notCommittedCommand;
        }

        public string TryGetNotCommitedCommand()
        {
            this.currentEntryIndex = this.entries.Count;
            return this.notCommittedCommand;
        }

        public string TryGetPreviousEntry(string currentInput)
        {
            if (this.currentEntryIndex <= 0)
            {
                // cannot go back
                return currentInput;
            }

            if (this.currentEntryIndex == this.entries.Count)
            {
                // save current input
                this.notCommittedCommand = currentInput;
            }

            // return previous entry
            this.currentEntryIndex--;
            return this.entries[this.currentEntryIndex];
        }

        private void Save()
        {
            Storage.Save(this.entries, writeToLog: false);
        }

        private List<string> TryLoad()
        {
            Storage.TryLoad(out List<string> result);
            return result ?? new List<string>();
        }
    }
}