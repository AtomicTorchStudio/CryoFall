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
        public const string Notification_CreatureDiscovered_MessageFormat
            = "Killed for the first time.";

        public const string Notification_FoodDiscovered_MessageFormat
            = "Consumed for the first time.";

        // Related to "Pry open" action on the loot containers and the "Searching" skill.
        public const string Notification_LootDiscovered_MessageFormat
            = "Found for the first time.";

        private static Dictionary<IProtoEntity, ViewDataEntryCompletionist> allFoodEntries;

        private static Dictionary<IProtoEntity, ViewDataEntryCompletionist> allLootEntries;

        private static Dictionary<IProtoEntity, ViewDataEntryCompletionist> allMobEntries;

        static ViewModelWindowCompletionist()
        {
        }

        private ViewModelWindowCompletionist()
        {
            var commandClaimReward = new ActionCommandWithParameter(
                proto => CompletionistSystem.ClientClaimReward((IProtoEntity)proto));

            if (allFoodEntries is null)
            {
                allFoodEntries = CompletionistSystem.CompletionistAllFood.ToDictionary(
                    proto => (IProtoEntity)proto,
                    proto => new ViewDataEntryCompletionist(proto,
                                                            commandClaimReward));
            }

            if (allMobEntries is null)
            {
                allMobEntries = CompletionistSystem.CompletionistAllMobs.ToDictionary(
                    proto => (IProtoEntity)proto,
                    proto => new ViewDataEntryCompletionist(proto,
                                                            commandClaimReward));
            }

            if (allLootEntries is null)
            {
                allLootEntries = CompletionistSystem.CompletionistAllLoot.ToDictionary(
                    proto => (IProtoEntity)proto,
                    proto => new ViewDataEntryCompletionist(proto,
                                                            commandClaimReward));
            }

            this.EntriesFood = new ViewModelCompletionistPage(
                allFoodEntries,
                columnsCount: 4,
                iconSize: 74,
                entriesPendingCountChanged: this.EntriesPendingCountChangedHandler);

            this.EntriesMobs = new ViewModelCompletionistPage(
                allMobEntries,
                columnsCount: 3,
                iconSize: 111,
                entriesPendingCountChanged: this.EntriesPendingCountChangedHandler);

            this.EntriesLoot = new ViewModelCompletionistPage(
                allLootEntries,
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

        public ViewModelCompletionistPage EntriesFood { get; }

        public ViewModelCompletionistPage EntriesLoot { get; }

        public ViewModelCompletionistPage EntriesMobs { get; }

        public bool HasPendingEntries { get; private set; }

        public int TotalPendingEntries { get; private set; }

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
            this.TotalPendingEntries = this.EntriesFood.PendingEntriesCount
                                       + this.EntriesMobs.PendingEntriesCount
                                       + this.EntriesLoot.PendingEntriesCount;
            this.HasPendingEntries = this.TotalPendingEntries > 0;
        }
    }
}