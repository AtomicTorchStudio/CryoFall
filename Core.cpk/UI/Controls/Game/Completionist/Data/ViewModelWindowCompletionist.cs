namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Completionist.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Completionist;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelWindowCompletionist : BaseViewModel
    {
        public const double NewEntryNotificationDelay = 1.5;

        public const string Notification_CreatureDiscovered_MessageFormat
            = "Killed for the first time.";

        public const string Notification_FishDiscovered_MessageFormat
            = "Caught for the first time.";

        public const string Notification_FoodDiscovered_MessageFormat
            = "Consumed for the first time.";

        // Related to "Pry open" action on the loot containers and the "Searching" skill.
        public const string Notification_LootDiscovered_MessageFormat
            = "Found for the first time.";

        private static Dictionary<IProtoEntity, ViewDataEntryFishCompletionist> allFishEntries;

        private static Dictionary<IProtoEntity, ViewDataEntryCompletionist> allFoodEntries;

        private static Dictionary<IProtoEntity, ViewDataEntryCompletionist> allLootEntries;

        private static Dictionary<IProtoEntity, ViewDataEntryCompletionist> allMobEntries;

        private bool hasPendingEntries;

        private int totalPendingEntries;

        static ViewModelWindowCompletionist()
        {
        }

        private ViewModelWindowCompletionist()
        {
            var commandClaimReward = new ActionCommandWithParameter(
                proto => CompletionistSystem.ClientClaimReward((IProtoEntity)proto));

            allFoodEntries ??= CompletionistSystem.CompletionistAllFood.ToDictionary(
                proto => (IProtoEntity)proto,
                proto => new ViewDataEntryCompletionist(proto,
                                                        commandClaimReward));

            allMobEntries ??= CompletionistSystem.CompletionistAllMobs.ToDictionary(
                proto => (IProtoEntity)proto,
                proto => new ViewDataEntryCompletionist(proto,
                                                        commandClaimReward));

            allLootEntries ??= CompletionistSystem.CompletionistAllLoot.ToDictionary(
                proto => (IProtoEntity)proto,
                proto => new ViewDataEntryCompletionist(proto,
                                                        commandClaimReward));

            allFishEntries ??= CompletionistSystem.CompletionistAllFish.ToDictionary(
                proto => (IProtoEntity)proto,
                proto => new ViewDataEntryFishCompletionist(proto,
                                                            commandClaimReward));

            this.EntriesFood = new ViewModelCompletionistPageDefault(
                allFoodEntries,
                columnsCount: 4,
                iconSize: 74,
                entriesPendingCountChanged: this.EntriesPendingCountChangedHandler);

            this.EntriesMobs = new ViewModelCompletionistPageDefault(
                allMobEntries,
                columnsCount: 3,
                iconSize: 111,
                entriesPendingCountChanged: this.EntriesPendingCountChangedHandler);

            this.EntriesLoot = new ViewModelCompletionistPageDefault(
                allLootEntries,
                columnsCount: 3,
                iconSize: 111,
                entriesPendingCountChanged: this.EntriesPendingCountChangedHandler);

            this.EntriesFish = new ViewModelCompletionistPageFish(
                allFishEntries,
                columnsCount: 3,
                iconSize: 111,
                entriesPendingCountChanged: this.EntriesPendingCountChangedHandler);

            BootstrapperClientGame.InitCallback += this.BootstrapperClientGameInitCallbackHandler;

            this.RefreshLists();
        }

        protected internal enum CompletionistEntryState
        {
            Undiscovered,

            RewardAvailable,

            RewardClaimed
        }

        public static ViewModelWindowCompletionist Instance { get; }
            = new ViewModelWindowCompletionist();

        public ViewModelCompletionistPageFish EntriesFish { get; }

        public ViewModelCompletionistPageDefault EntriesFood { get; }

        public ViewModelCompletionistPageDefault EntriesLoot { get; }

        public ViewModelCompletionistPageDefault EntriesMobs { get; }

        public bool HasPendingEntries
        {
            get => this.hasPendingEntries;
        }

        public int TotalPendingEntries
        {
            get => this.totalPendingEntries;
        }

        public void RefreshLists()
        {
            this.EntriesFood.Source = null;
            this.EntriesMobs.Source = null;
            this.EntriesLoot.Source = null;

            if (ClientCurrentCharacterHelper.Character is null)
            {
                return;
            }

            var data = ClientCurrentCharacterHelper.PrivateState.CompletionistData;
            this.EntriesFood.Source = data.ListFood;
            this.EntriesMobs.Source = data.ListMobs;
            this.EntriesLoot.Source = data.ListLoot;
            this.EntriesFish.Source = data.ListFish;
        }

        protected override void DisposeViewModel()
        {
            BootstrapperClientGame.InitCallback -= this.BootstrapperClientGameInitCallbackHandler;
        }

        private void BootstrapperClientGameInitCallbackHandler(ICharacter character)
        {
            this.RefreshLists();
        }

        private void EntriesPendingCountChangedHandler()
        {
            var previousNumber = this.totalPendingEntries;
            this.totalPendingEntries = this.EntriesFood.PendingEntriesCount
                                       + this.EntriesMobs.PendingEntriesCount
                                       + this.EntriesLoot.PendingEntriesCount
                                       + this.EntriesFish.PendingEntriesCount;
            this.hasPendingEntries = this.TotalPendingEntries > 0;

            ClientTimersSystem.AddAction(
                delaySeconds: previousNumber < this.totalPendingEntries
                                  ? NewEntryNotificationDelay
                                  : 0,
                () =>
                {
                    this.NotifyPropertyChanged(nameof(this.TotalPendingEntries));
                    this.NotifyPropertyChanged(nameof(this.HasPendingEntries));
                });
        }
    }
}