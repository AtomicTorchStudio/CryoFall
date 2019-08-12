namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ViewModelCraftingMenuRecipeDetails : BaseViewModel
    {
        private static readonly IReadOnlyList<ProtoItemWithCount> EmptyItemsList
            = new List<ProtoItemWithCount>(0).AsReadOnly();

        private static readonly IInputClientService Input = Client.Input;

        private readonly ICharacter character;

        private readonly Action<Recipe> customCallbackOnRecipeSelect;

        private readonly bool validateItemsAvailabilityInPlayerInventory;

        private ushort countToCraft = 1;

        private string countToCraftString;

        private bool isCanCraft;

        private bool isUnlocked = true;

        private ViewModelCraftingRecipe viewModelRecipe;

        public ViewModelCraftingMenuRecipeDetails(
            bool validateItemsAvailabilityInPlayerInventory,
            Action<Recipe> customCallbackOnRecipeSelect = null)
        {
            this.validateItemsAvailabilityInPlayerInventory = validateItemsAvailabilityInPlayerInventory;
            this.customCallbackOnRecipeSelect = customCallbackOnRecipeSelect;
            this.countToCraftString = this.countToCraft.ToString();

            if (IsDesignTime)
            {
                return;
            }

            this.character = Client.Characters.CurrentPlayerCharacter;

            this.CommandCraftingCountIncrease = new ActionCommand(() => this.AddCraftingCount(+1));
            this.CommandCraftingCountDecrease = new ActionCommand(() => this.AddCraftingCount(-1));
            this.CommandCraftingCountSetOne = new ActionCommand(() => this.CountToCraft = 1);
            this.CommandCraftingCountSetMax = new ActionCommand(this.SetMaximumCraftingCount);
            this.CommandCraft = new ActionCommandWithCondition(
                this.ExecuteCommandCraft,
                checkCanExecute: () => this.IsCanCraft);

            this.CommandCraftByDoubleClick = new ActionCommandWithCondition(
                this.ExecuteCommandCraftByDoubleClick,
                checkCanExecute: () => this.IsCanCraft);

            ClientCurrentCharacterContainersHelper.ItemAddedOrRemovedOrCountChanged
                += this.CharacterContainersItemsChangedHandler;
        }

        public BaseCommand CommandCraft { get; }

        public BaseCommand CommandCraftByDoubleClick { get; }

        public BaseCommand CommandCraftingCountDecrease { get; }

        public BaseCommand CommandCraftingCountIncrease { get; }

        public BaseCommand CommandCraftingCountSetMax { get; }

        public BaseCommand CommandCraftingCountSetOne { get; }

        public ushort CountToCraft
        {
            get => this.countToCraft;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }

                if (this.viewModelRecipe == null)
                {
                    return;
                }

                var protoItem = this.viewModelRecipe.Recipe.OutputItems.Items[0].ProtoItem;
                if (protoItem.IsStackable)
                {
                    if (value > protoItem.MaxItemsPerStack)
                    {
                        value = protoItem.MaxItemsPerStack;
                    }
                }
                else
                {
                    var maxCraftingQueueEntriesCount = CraftingSystem.ClientCurrentMaxCraftingQueueEntriesCount;
                    if (value > maxCraftingQueueEntriesCount)
                    {
                        value = maxCraftingQueueEntriesCount;
                    }
                }

                if (this.countToCraft == value)
                {
                    return;
                }

                this.countToCraft = value;
                this.CountToCraftString = value.ToString();
                this.NotifyThisPropertyChanged();
                this.RefreshIsCanCraft();
            }
        }

        public string CountToCraftString
        {
            get => this.countToCraftString;
            set
            {
                if (this.countToCraftString == value)
                {
                    return;
                }

                this.countToCraftString = value;
                this.NotifyThisPropertyChanged();

                if (string.IsNullOrEmpty(value))
                {
                    // do not update CountToCraft - nothing is entered
                    return;
                }

                if (ushort.TryParse(value, out var parsed))
                {
                    // correct number value entered - update CountToCraft
                    this.CountToCraft = parsed;
                }
                else
                {
                    // incorrect number value entered (non-parseable) - reset text to current value
                    this.CountToCraftString = this.CountToCraft.ToString();
                }
            }
        }

        public IReadOnlyList<ProtoItemWithCount> InputItems { get; set; } = EmptyItemsList;

        public bool IsCanCraft
        {
            get => this.isCanCraft;
            set
            {
                if (this.isCanCraft == value)
                {
                    return;
                }

                this.isCanCraft = value;
                this.NotifyThisPropertyChanged();
                this.CommandCraft.OnCanExecuteChanged();
            }
        }

        /// <summary>
        /// Calculates maximum crafting count (considering how much resources the player had)
        /// </summary>
        public ushort MaximumCraftingCount
        {
            get
            {
                var recipe = this.viewModelRecipe?.Recipe;
                if (recipe == null)
                {
                    // cannot craft at all
                    return 0;
                }

                if (CreativeModeSystem.SharedIsInCreativeMode(this.character))
                {
                    return 1000;
                }

                // Enumerate over all the recipe input items and find out
                // which items are limiting the maximum crafting count.
                var result = 1000;
                foreach (var protoItemWithCount in recipe.InputItems)
                {
                    var itemAvailableCount = this.character.CountItemsOfType(protoItemWithCount.ProtoItem);
                    var canCraftCount = itemAvailableCount / protoItemWithCount.Count;

                    if (canCraftCount < result)
                    {
                        result = canCraftCount;
                    }
                    else if (canCraftCount == 0)
                    {
                        // don't have required count of this item - cannot craft at all
                        return 0;
                    }
                }

                return (ushort)result;
            }
        }

        public IReadOnlyList<ProtoItemWithCount> OutputItems { get; set; } = EmptyItemsList;

        public ViewModelCraftingRecipe ViewModelRecipe
        {
            get => this.viewModelRecipe;
            set
            {
                if (this.viewModelRecipe == value)
                {
                    // refresh input/output items
                    this.NotifyPropertyChanged(nameof(this.InputItems));
                    this.NotifyPropertyChanged(nameof(this.OutputItems));
                    return;
                }

                this.viewModelRecipe = value;

                var recipe = value?.Recipe;
                this.InputItems = recipe?.InputItems ?? EmptyItemsList;
                this.OutputItems = recipe?.OutputItems.Items ?? EmptyItemsList;

                // ensure the property will be set
                this.countToCraft = 0;
                // set property
                this.CountToCraft = 1;
                // no need - it's called automatically in during property set
                //this.RefreshIsCanCraft();

                this.NotifyThisPropertyChanged();
            }
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();

            ClientCurrentCharacterContainersHelper.ItemAddedOrRemovedOrCountChanged
                -= this.CharacterContainersItemsChangedHandler;
        }

        private void AddCraftingCount(int delta)
        {
            if (Input.IsKeyHeld(InputKey.Shift, evenIfHandled: true))
            {
                delta *= 5;
            }

            var newValue = delta + this.countToCraft;
            newValue = MathHelper.Clamp(newValue, 1, this.MaximumCraftingCount);
            this.CountToCraft = (ushort)newValue;
        }

        private void CharacterContainersItemsChangedHandler(IItem item)
        {
            this.RefreshIsCanCraft();
        }

        private async void ExecuteCommandCraft()
        {
            if (!this.IsCanCraft)
            {
                return;
            }

            if (this.customCallbackOnRecipeSelect != null)
            {
                this.customCallbackOnRecipeSelect.Invoke(this.viewModelRecipe.Recipe);
            }
            else
            {
                await CraftingSystem.ClientStartCrafting(this.viewModelRecipe.Recipe, this.countToCraft);
            }

            // after starting craft - automatically limit count to craft to the maximum value
            var max = this.MaximumCraftingCount;
            if (this.CountToCraft > max)
            {
                this.CountToCraft = max;
            }
        }

        private void ExecuteCommandCraftByDoubleClick()
        {
            var originalCountToCraft = this.CountToCraft;
            try
            {
                if (Input.IsKeyHeld(InputKey.Shift,  evenIfHandled: true)
                    || Input.IsKeyHeld(InputKey.Alt, evenIfHandled: true))
                {
                    // quick craft max except the case with non-cancelable items
                    this.CountToCraft = this.viewModelRecipe.Recipe.IsCancellable
                                            ? this.MaximumCraftingCount
                                            : (ushort)5;
                }
                else
                {
                    // quick craft a single item
                    this.CountToCraft = 1;
                }

                if (!this.IsCanCraft)
                {
                    return;
                }

                this.ExecuteCommandCraft();
            }
            finally
            {
                // restore the original count
                this.CountToCraft = originalCountToCraft;
            }
        }

        private void RefreshIsCanCraft()
        {
            var recipe = this.viewModelRecipe?.Recipe;
            if (recipe == null)
            {
                this.IsCanCraft = false;
                return;
            }

            if (CreativeModeSystem.SharedIsInCreativeMode(this.character))
            {
                this.IsCanCraft = true;
                return;
            }

            if (this.validateItemsAvailabilityInPlayerInventory)
            {
                foreach (var protoItemWithCount in recipe.InputItems)
                {
                    if (!this.character.ContainsItemsOfType(
                            protoItemWithCount.ProtoItem,
                            protoItemWithCount.Count * (uint)this.CountToCraft))
                    {
                        // don't have a required item
                        this.IsCanCraft = false;
                        return;
                    }
                }

                // can craft - has all the required items
            }

            // but need to check if it's unlocked
            this.IsCanCraft = this.isUnlocked;
        }

        private void SetMaximumCraftingCount()
        {
            this.CountToCraft = this.MaximumCraftingCount;
        }
    }
}