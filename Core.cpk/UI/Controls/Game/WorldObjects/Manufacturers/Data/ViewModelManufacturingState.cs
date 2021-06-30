namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Manufacturers.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Managers;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelManufacturingState : BaseViewModel
    {
        public readonly ManufacturingConfig ManufacturingConfig;

        public readonly ManufacturingState ManufacturingState;

        private readonly NetworkSyncList<CraftingQueueItem> craftingQueue;

        private readonly IStaticWorldObject worldObjectManufacturer;

        private Recipe bestMatchingRecipe;

        private ViewModelCraftingRecipe bestMatchingRecipeViewModel;

        private Recipe selectedRecipe;

        private ViewModelCraftingRecipe selectedRecipeViewModel;

        private Visibility visibilityPrimary = Visibility.Visible;

        private Visibility visibilityRecipesBrowser = Visibility.Collapsed;

        public ViewModelManufacturingState(
            IStaticWorldObject worldObjectManufacturer,
            ManufacturingState manufacturingState,
            ManufacturingConfig manufacturingConfig)
        {
            this.worldObjectManufacturer = worldObjectManufacturer;
            this.ManufacturingState = manufacturingState;
            this.ManufacturingConfig = manufacturingConfig;

            this.ContainerInput = (IClientItemsContainer)manufacturingState.ContainerInput;
            this.ContainerOutput = (IClientItemsContainer)manufacturingState.ContainerOutput;

            this.CommandBrowseRecipes = new ActionCommand(this.ExecuteCommandBrowseRecipes);
            this.CommandCloseRecipesBrowser = new ActionCommand(this.CloseRecipesBrowser);
            this.CommandSelectRecipeFromBrowser =
                new ActionCommandWithParameter(this.ExecuteCommandSelectRecipeFromBrowser);
            this.CommandApplyBestMatchingRecipe = new ActionCommand(this.ExecuteCommandApplyBestMatchingRecipe);

            this.VisibilityRecipesSelection = manufacturingConfig.IsAutoSelectRecipe
                                                  ? Visibility.Collapsed
                                                  : Visibility.Visible;

            this.SelectedRecipe = manufacturingState.SelectedRecipe;
            //this.BestMatchingRecipe = manufacturingState.BestMatchingRecipe;

            this.RefreshCraftingProgressPercents();

            this.ManufacturingState.ClientSubscribe(
                _ => _.SelectedRecipe,
                recipe => this.SelectedRecipe = recipe,
                this);

            //this.ManufacturingState.ClientSubscribe(
            //    _ => _.BestMatchingRecipe,
            //    recipe => this.BestMatchingRecipe = recipe,
            //    this);

            this.ManufacturingState.CraftingQueue.ClientSubscribe(
                _ => _.IsContainerOutputFull,
                _ => this.NotifyPropertyChanged(nameof(this.IsContainerOutputFull)),
                this);

            this.craftingQueue = this.ManufacturingState.CraftingQueue.QueueItems;
            this.craftingQueue.ClientAnyModification
                += this.CraftingQueueAnyModificationHandler;

            this.ManufacturingState.CraftingQueue.ClientSubscribe(
                _ => _.TimeRemainsToComplete,
                time => this.RefreshCraftingProgressPercents(),
                this);

            // register containers exchange
            var character = Client.Characters.CurrentPlayerCharacter;
            ClientContainersExchangeManager.Register(
                this,
                this.ManufacturingState.ContainerOutput,
                allowedTargets: new[]
                {
                    character.SharedGetPlayerContainerInventory(),
                    character.SharedGetPlayerContainerHotbar()
                });

            ClientContainersExchangeManager.Register(
                this,
                this.ManufacturingState.ContainerInput,
                allowedTargets: new[]
                {
                    character.SharedGetPlayerContainerInventory(),
                    character.SharedGetPlayerContainerHotbar()
                });

            this.ContainerInput.StateHashChanged += this.ContainerInputStateChangedHandler;

            this.RefreshIsInputMatchSelectedRecipe();
            this.RefreshBestMatchingRecipe();
        }

        public Recipe BestMatchingRecipe
        {
            get => this.bestMatchingRecipe;
            set
            {
                if (this.bestMatchingRecipe == value)
                {
                    return;
                }

                if (value is not null
                    && !value.SharedIsTechUnlocked(Client.Characters.CurrentPlayerCharacter))
                {
                    // suggested recipe is locked
                    value = null;
                }

                this.bestMatchingRecipe = value;
                this.NotifyThisPropertyChanged();
                this.BestMatchingRecipeViewModel = value is null ? null : new ViewModelCraftingRecipe(value);

                this.RefreshIsInputMatchSelectedRecipe();
            }
        }

        public ViewModelCraftingRecipe BestMatchingRecipeViewModel
        {
            get => this.bestMatchingRecipeViewModel;
            set
            {
                this.bestMatchingRecipeViewModel?.Dispose();
                this.bestMatchingRecipeViewModel = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public BaseCommand CommandApplyBestMatchingRecipe { get; }

        public BaseCommand CommandBrowseRecipes { get; }

        public BaseCommand CommandCloseRecipesBrowser { get; }

        public ActionCommandWithParameter CommandSelectRecipeFromBrowser { get; }

        public IClientItemsContainer ContainerInput { get; }

        public IClientItemsContainer ContainerOutput { get; }

        public float CraftingProgressPercents { get; private set; } = 50f;

        public bool IsContainerOutputFull => this.ManufacturingState.CraftingQueue.IsContainerOutputFull;

        public bool IsInputMatchSelectedRecipe { get; private set; }

        public bool IsInputNotEnoughItems { get; private set; }

        public ViewModelCraftingMenu RecipesBrowserViewModel { get; private set; }

        public Recipe SelectedRecipe
        {
            get => this.selectedRecipe;
            private set
            {
                if (this.selectedRecipe == value)
                {
                    return;
                }

                this.selectedRecipe = value;
                this.NotifyThisPropertyChanged();
                this.SelectedRecipeViewModel = value is null ? null : new ViewModelCraftingRecipe(value);

                this.RefreshBestMatchingRecipe();
                this.RefreshIsInputMatchSelectedRecipe();
            }
        }

        public ViewModelCraftingRecipe SelectedRecipeViewModel
        {
            get => this.selectedRecipeViewModel;
            set
            {
                this.selectedRecipeViewModel?.Dispose();
                this.selectedRecipeViewModel = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public Visibility VisibilityPrimary
        {
            get => this.visibilityPrimary;
            private set
            {
                if (this.visibilityPrimary == value)
                {
                    return;
                }

                this.visibilityPrimary = value;
                this.NotifyThisPropertyChanged();

                // change recipes browser visibility
                this.VisibilityRecipesBrowser = value != Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility VisibilityRecipesBrowser
        {
            get => this.visibilityRecipesBrowser;
            private set
            {
                if (this.visibilityRecipesBrowser == value)
                {
                    return;
                }

                this.visibilityRecipesBrowser = value;
                this.NotifyThisPropertyChanged();

                // change primary visibility
                this.VisibilityPrimary = value != Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;

                if (value == Visibility.Visible)
                {
                    var recipes = this.GetAllRecipes();
                    var recipesCountTotal = recipes.Count;
                    this.RemoveLockedRecipes(recipes);

                    this.RecipesBrowserViewModel = new ViewModelCraftingMenu(
                        recipes,
                        recipesCountTotal,
                        validateItemsAvailabilityInPlayerInventory: false,
                        customCallbackOnRecipeSelect: this.ExecuteCommandSelectRecipeFromBrowser);

                    if (this.selectedRecipe is not null)
                    {
                        this.RecipesBrowserViewModel.SelectedRecipe =
                            this.RecipesBrowserViewModel.RecipesList.FirstOrDefault(
                                r => r.ViewModelRecipe.Recipe == this.selectedRecipe)?.ViewModelRecipe;
                    }
                }
                else
                {
                    var disposable = this.RecipesBrowserViewModel;
                    this.RecipesBrowserViewModel = null;
                    disposable?.Dispose();
                }
            }
        }

        public Visibility VisibilityRecipesSelection { get; }

        public void RefreshIsInputMatchSelectedRecipe()
        {
            var occupiedSlotsCount = this.ContainerInput.OccupiedSlotsCount;

            if (this.selectedRecipe is null
                || occupiedSlotsCount == 0)
            {
                this.IsInputNotEnoughItems = false;

                // consider the recipe is matched if there is no input
                if (this.selectedRecipe is null
                    && occupiedSlotsCount > 0
                    && this.bestMatchingRecipe is null)
                {
                    this.IsInputMatchSelectedRecipe = false;
                    return;
                }

                this.IsInputMatchSelectedRecipe = true;
                return;
            }

            var isInputMatchSelectedRecipe = this.selectedRecipe.CanBeCrafted(
                character: null,
                this.worldObjectManufacturer,
                this.ManufacturingState.CraftingQueue,
                countToCraft: 1);

            if (isInputMatchSelectedRecipe)
            {
                this.IsInputMatchSelectedRecipe = true;
                this.IsInputNotEnoughItems = false;
                return;
            }

            // input doesn't match the selected recipe
            // make extra check whether the recipe is actually doesn't match or there are simply not enough items
            isInputMatchSelectedRecipe = true;
            var selectedRecipeInputItems = this.selectedRecipe.InputItems;
            foreach (var craftingInputItem in selectedRecipeInputItems)
            {
                foreach (var inputItemsContainer in this.ManufacturingState.CraftingQueue.InputContainersArray)
                {
                    if (!inputItemsContainer.ContainsItemsOfType(
                            craftingInputItem.ProtoItem,
                            requiredCount: 1,
                            out _))
                    {
                        // required item is not available
                        isInputMatchSelectedRecipe = false;
                        break;
                    }
                }
            }

            if (!isInputMatchSelectedRecipe)
            {
                // check for the special case - if all the container items are contained in the recipe
                // then we should display "not enough items for current recipe"
                isInputMatchSelectedRecipe = true;
                foreach (var itemsContainer in this.ManufacturingState.CraftingQueue.InputContainersArray)
                {
                    foreach (var item in itemsContainer.Items)
                    {
                        var isItemAvailableInRecipe = false;
                        foreach (var inputItem in selectedRecipeInputItems)
                        {
                            if (inputItem.ProtoItem == item.ProtoItem)
                            {
                                isItemAvailableInRecipe = true;
                                break;
                            }
                        }

                        if (!isItemAvailableInRecipe)
                        {
                            // found foreign item
                            isInputMatchSelectedRecipe = false;
                            break;
                        }
                    }
                }
            }

            this.IsInputMatchSelectedRecipe = isInputMatchSelectedRecipe;
            this.IsInputNotEnoughItems = isInputMatchSelectedRecipe;
        }

        protected override void DisposeViewModel()
        {
            base.DisposeViewModel();
            this.craftingQueue.ClientAnyModification -= this.CraftingQueueAnyModificationHandler;
            this.ContainerInput.StateHashChanged -= this.ContainerInputStateChangedHandler;
        }

        private void CloseRecipesBrowser()
        {
            this.VisibilityRecipesBrowser = Visibility.Collapsed;
        }

        private void ContainerInputStateChangedHandler()
        {
            this.RefreshBestMatchingRecipe();
            this.RefreshIsInputMatchSelectedRecipe();
        }

        private void CraftingQueueAnyModificationHandler(NetworkSyncList<CraftingQueueItem> source)
        {
            this.RefreshCraftingProgressPercents();
        }

        private void ExecuteCommandApplyBestMatchingRecipe()
        {
            this.SelectRecipe(this.bestMatchingRecipe);
        }

        private void ExecuteCommandBrowseRecipes()
        {
            this.VisibilityRecipesBrowser = Visibility.Visible;
        }

        private void ExecuteCommandSelectRecipeFromBrowser(object arg)
        {
            var recipe = (Recipe)arg;
            this.CloseRecipesBrowser();
            this.SelectRecipe(recipe);
        }

        private List<Recipe> GetAllRecipes()
        {
            var list = this.ManufacturingConfig.Recipes.ToList();
            list.SortBy(r => r.ShortId, StringComparer.Ordinal);
            return list;
        }

        private void RefreshBestMatchingRecipe()
        {
            if (this.ManufacturingConfig.IsAutoSelectRecipe)
            {
                this.BestMatchingRecipe = null;
                return;
            }

            this.BestMatchingRecipe = ManufacturingMechanic.SharedMatchBestRecipe(this.ManufacturingState,
                this.ManufacturingConfig,
                this.worldObjectManufacturer);
        }

        private void RefreshCraftingProgressPercents()
        {
            var craftingQueue = this.ManufacturingState.CraftingQueue;
            var item = craftingQueue.QueueItems.FirstOrDefault();
            if (item?.Recipe is null)
            {
                this.CraftingProgressPercents = 0;
                return;
            }

            var totalDuration = item.Recipe.OriginalDuration;
            this.CraftingProgressPercents = (float)(100
                                                    * (totalDuration - craftingQueue.TimeRemainsToComplete)
                                                    / totalDuration);
        }

        private void RemoveLockedRecipes(List<Recipe> list)
        {
            var character = Client.Characters.CurrentPlayerCharacter;
            list.RemoveAll(r => !r.SharedIsTechUnlocked(character));
        }

        private void SelectRecipe(Recipe recipe)
        {
            var protoManufacturer = (IProtoObjectManufacturer)this.worldObjectManufacturer.ProtoStaticWorldObject;
            protoManufacturer.ClientSelectRecipe(this.worldObjectManufacturer, recipe);
        }
    }
}