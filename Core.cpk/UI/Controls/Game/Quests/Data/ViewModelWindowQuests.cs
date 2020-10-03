namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Quests.Data
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Quests;
    using AtomicTorch.CBND.CoreMod.Systems.Quests;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelWindowQuests : BaseViewModel
    {
        private static readonly SoundResource SoundResourceQuestUnlocked =
            new SoundResource("UI/Quests/QuestUnlocked.ogg");

        private static ulong lastQuestUnlockedFrame;

        public ViewModelWindowQuests()
        {
            this.TotalQuestsCount = Api.FindProtoEntities<IProtoQuest>().Count;

            var questsList = ClientCurrentCharacterHelper.PrivateState.Quests.Quests;
            questsList.ClientElementInserted += this.QuestsListElementInserted;
            questsList.ClientElementRemoved += this.QuestsListElementRemoved;

            var viewModels = questsList
                             .Select(q => new ViewModelQuestEntry(q,
                                                                  this.ViewModelQuestEntryOnCompletedStateChanged))
                             .ToList();

            this.ActiveQuests = new ObservableCollection<ViewModelQuestEntry>(
                viewModels.Where(vm => !vm.QuestEntry.IsCompleted));

            var firstQuest = this.ActiveQuests.FirstOrDefault();
            firstQuest?.SetIsCollapsed(false, removeNewFlag: false);

            this.CompletedQuests = new ObservableCollection<ViewModelQuestEntry>(
                viewModels.Where(vm => vm.QuestEntry.IsCompleted)
                          .Reverse());
        }

        public ObservableCollection<ViewModelQuestEntry> ActiveQuests { get; }

        public ObservableCollection<ViewModelQuestEntry> CompletedQuests { get; }

        public int TotalQuestsCount { get; }

        public int UnlockedQuestsCount => this.ActiveQuests.Count
                                          + this.CompletedQuests.Count;

        public void RemoveNewFlagFromTheExpandedQuests()
        {
            foreach (var viewModelQuestEntry in this.ActiveQuests)
            {
                if (!viewModelQuestEntry.IsCollapsed
                    && viewModelQuestEntry.IsNew)
                {
                    viewModelQuestEntry.RemoveNewFlag();
                }
            }
        }

        private void QuestsListElementInserted(
            NetworkSyncList<PlayerCharacterQuests.CharacterQuestEntry> source,
            int index,
            PlayerCharacterQuests.CharacterQuestEntry value)
        {
            var vm = new ViewModelQuestEntry(value, this.ViewModelQuestEntryOnCompletedStateChanged);
            if (vm.QuestEntry.IsCompleted)
            {
                this.CompletedQuests.Insert(0, vm);
                if (this.CompletedQuests.Count == 1)
                {
                    // display completed quests
                    this.NotifyPropertyChanged(nameof(this.CompletedQuests));
                }

                this.NotifyPropertyChanged(nameof(this.UnlockedQuestsCount));
                return;
            }

            // add new active quest
            // make it expanded by default
            vm.SetIsCollapsed(false, removeNewFlag: false);
            this.ActiveQuests.Add(vm);
            if (this.ActiveQuests.Count == 1)
            {
                // remove no active quests label
                this.NotifyPropertyChanged(nameof(this.ActiveQuests));
            }

            // play quest unlocked sound (not more often than once per frame)
            if (lastQuestUnlockedFrame != Client.CurrentGame.ServerFrameNumber)
            {
                lastQuestUnlockedFrame = Client.CurrentGame.ServerFrameNumber;
                Api.Client.Audio.PlayOneShot(SoundResourceQuestUnlocked, volume: 0.5f);
            }

            this.NotifyPropertyChanged(nameof(this.UnlockedQuestsCount));
        }

        private void QuestsListElementRemoved(
            NetworkSyncList<PlayerCharacterQuests.CharacterQuestEntry> source,
            int index,
            PlayerCharacterQuests.CharacterQuestEntry removedValue)
        {
            TryRemoveFrom(this.ActiveQuests);
            TryRemoveFrom(this.CompletedQuests);

            void TryRemoveFrom(ObservableCollection<ViewModelQuestEntry> list)
            {
                var viewModel = list.FirstOrDefault(vm => vm.QuestEntry == removedValue);
                if (viewModel is not null)
                {
                    list.Remove(viewModel);
                    viewModel.Dispose();
                }
            }

            this.NotifyPropertyChanged(nameof(this.UnlockedQuestsCount));
        }

        private void ViewModelQuestEntryOnCompletedStateChanged(ViewModelQuestEntry viewModel)
        {
            if (viewModel.QuestEntry.IsCompleted)
            {
                if (!this.ActiveQuests.Contains(viewModel))
                {
                    return;
                }

                // the quest is completed now
                // move the quest to the completed quests list
                this.ActiveQuests.Remove(viewModel);
                if (this.ActiveQuests.Count == 0)
                {
                    // display no active quests label
                    this.NotifyPropertyChanged(nameof(this.ActiveQuests));
                }

                viewModel.SetIsCollapsed(true);
                this.CompletedQuests.Insert(0, viewModel);
                if (this.CompletedQuests.Count == 1)
                {
                    // display completed quests
                    this.NotifyPropertyChanged(nameof(this.CompletedQuests));
                }

                this.NotifyPropertyChanged(nameof(this.UnlockedQuestsCount));
                return;
            }

            // the quest is not completed
            if (!this.CompletedQuests.Contains(viewModel))
            {
                return;
            }

            // move the quest to the active quests list
            this.CompletedQuests.Remove(viewModel);
            if (this.CompletedQuests.Count == 0)
            {
                // hide completed quests
                this.NotifyPropertyChanged(nameof(this.CompletedQuests));
            }

            viewModel.SetIsCollapsed(true);
            this.ActiveQuests.Add(viewModel);
            if (this.ActiveQuests.Count == 1)
            {
                // remove no active quests label
                this.NotifyPropertyChanged(nameof(this.ActiveQuests));
            }

            this.NotifyPropertyChanged(nameof(this.UnlockedQuestsCount));
        }
    }
}