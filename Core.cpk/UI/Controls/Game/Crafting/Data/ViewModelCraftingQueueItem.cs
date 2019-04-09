namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ViewModelCraftingQueueItem : BaseViewModel
    {
        private readonly Action countToCraftChangedCallback;

        private readonly CraftingQueue craftingQueue;

        private readonly CraftingQueueItem craftingQueueItem;

        private ushort? lastCountToCraft;

        public ViewModelCraftingQueueItem(
            CraftingQueue craftingQueue,
            CraftingQueueItem craftingQueueItem,
            Action countToCraftChangedCallback)
        {
            if (IsDesignTime)
            {
                this.Icon = Brushes.BlueViolet;
                return;
            }

            this.craftingQueue = craftingQueue;
            this.craftingQueueItem = craftingQueueItem;
            this.countToCraftChangedCallback = countToCraftChangedCallback;
            var outputProtoItem = this.craftingQueueItem.Recipe.OutputItems.Items.First().ProtoItem;

            this.RecipeViewModel = new ViewModelCraftingRecipe(this.craftingQueueItem.Recipe);

            // TODO: it's not a great idea to always send this info from the server,
            // it could be calculated on Client-side with custom component (updates every frame)
            // and only sometimes sync with the server.
            craftingQueue.ClientSubscribe(
                _ => _.TimeRemainsToComplete,
                newValue => { this.UpdateProgress(); },
                this);

            this.UpdateProgress();

            this.craftingQueueItem.ClientSubscribe(
                _ => _.CountToCraftRemains,
                newCountToCraftRemains =>
                {
                    if (this.lastCountToCraft.HasValue
                        && this.lastCountToCraft.Value > newCountToCraftRemains)
                    {
                        // count to craft decreased
                        Client.Audio.PlayOneShot(new SoundResource("UI/Notifications/ItemAdded"));
                    }

                    this.UpdateCountToCraftRemains();
                    countToCraftChangedCallback?.Invoke();
                },
                this);

            this.UpdateCountToCraftRemains();

            this.Icon = Client.UI.GetTextureBrush(outputProtoItem.Icon);
        }

#if !GAME

        public ViewModelCraftingQueueItem()
        {
        }

#endif

        public Brush Icon { get; }

        public int LocalId => this.craftingQueueItem.LocalId;

        public byte ProgressPercents { get; private set; }

        public Visibility ProgressVisibility { get; private set; }

        public ViewModelCraftingRecipe RecipeViewModel { get; }

        public string TextCountToCraftRemains { get; set; } = "1x";

        private void UpdateCountToCraftRemains()
        {
            this.lastCountToCraft = this.craftingQueueItem.CountToCraftRemains;

            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            this.TextCountToCraftRemains = this.craftingQueueItem.CountToCraftRemains > 1
                                               ? this.craftingQueueItem.CountToCraftRemains + "x"
                                               : string.Empty;
        }

        private void UpdateProgress()
        {
            if (this.craftingQueue.QueueItems.FirstOrDefault() != this.craftingQueueItem)
            {
                // another queue item crafted
                this.ProgressVisibility = Visibility.Collapsed;
                return;
            }

            var durationSeconds = this.craftingQueueItem.Recipe.SharedGetDurationForPlayer(
                ClientCurrentCharacterHelper.Character);
            var timeRemainsSeconds = this.craftingQueue.TimeRemainsToComplete;
            timeRemainsSeconds = MathHelper.Clamp(timeRemainsSeconds, 0, durationSeconds);
            var progress = (durationSeconds - timeRemainsSeconds) / durationSeconds;
            this.ProgressPercents = (byte)(100d * progress);
            this.ProgressVisibility = Visibility.Visible;
        }
    }
}