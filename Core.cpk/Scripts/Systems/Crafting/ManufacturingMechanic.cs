namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Logging;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// Mechanic for automated items manufacturing (in special buildings).
    /// </summary>
    public static class ManufacturingMechanic
    {
        private static readonly ILogger Logger = Api.Logger;

        /// <summary>
        /// Selects recipe for manufacturing.
        /// </summary>
        public static void SelectRecipe(
            Recipe recipe,
            IStaticWorldObject objectManufacturer,
            ManufacturingState state,
            ManufacturingConfig config)
        {
            if (state.SelectedRecipe == recipe)
            {
                return;
            }

            state.SelectedRecipe = recipe;
            state.CraftingQueue.QueueItems.Clear();

            RefreshRecipe(state, config, objectManufacturer);
            //RefreshFuel(state, config, objectManufacturer);
        }

        /// <summary>
        /// Matches best recipe for the current input items.
        /// Please note it will return null if there best recipe is already selected.
        /// Please note it will not return better recipe if currently
        /// there is a selected recipe which using more input slots.
        /// </summary>
        public static Recipe SharedMatchBestRecipe(
            ManufacturingState state,
            ManufacturingConfig config,
            IStaticWorldObject objectManufacturer)
        {
            var selectedRecipe = state.SelectedRecipe;
            var bestRecipe = config.MatchRecipe(objectManufacturer, state.CraftingQueue);

            if (selectedRecipe == bestRecipe)
            {
                return null;
            }

            if (bestRecipe is not null
                && selectedRecipe is not null
                && selectedRecipe.CanBeCrafted(objectManufacturer, state.CraftingQueue, 1)
                && bestRecipe.InputItems.Length <= selectedRecipe.InputItems.Length)
            {
                // the best recipe is ignored because selected recipe is also valid
                // and it has the same or more amount of input items
                return null;
            }

            return bestRecipe;
        }

        /// <summary>
        /// Performs full update of manufacturing state - equivalent to calling <see cref="UpdateRecipeOnly" />
        /// and then <see cref="UpdateCraftingQueueOnly" />.
        /// </summary>
        /// <param name="objectManufacturer">Instance of world object performing manufacturing.</param>
        /// <param name="state">Instance of manufacturing state.</param>
        /// <param name="config">Manufacturing config.</param>
        /// <param name="deltaTime">Delta time to progress on.</param>
        public static void Update(
            IStaticWorldObject objectManufacturer,
            ManufacturingState state,
            ManufacturingConfig config,
            double deltaTime)
        {
            UpdateRecipeOnly(objectManufacturer, state, config);
            UpdateCraftingQueueOnly(state, deltaTime);
        }

        /// <summary>
        /// Updates only crafting queue recipe - please be sure to call <see cref="UpdateRecipeOnly" /> before that.
        /// </summary>
        /// <param name="state">Instance of manufacturing state.</param>
        /// <param name="deltaTime">Delta time to progress on.</param>
        public static void UpdateCraftingQueueOnly(ManufacturingState state, double deltaTime)
        {
            CraftingMechanics.ServerUpdate(state.CraftingQueue, deltaTime);
        }

        /// <summary>
        /// Updates only current recipe - useful if you need to check if there any active recipe before doing other logic.
        /// </summary>
        /// <param name="objectManufacturer">Instance of world object performing manufacturing.</param>
        /// <param name="state">Instance of manufacturing state.</param>
        /// <param name="config">Manufacturing config.</param>
        public static void UpdateRecipeOnly(
            IStaticWorldObject objectManufacturer,
            ManufacturingState state,
            ManufacturingConfig config,
            bool force = false)
        {
            var containerInputStateHash = state.ContainerInput.StateHash;
            var containerOutputStateHash = state.ContainerOutput.StateHash;

            var containerInputChanged = state.ContainerInputLastStateHash != containerInputStateHash;
            var containerOutputChanged = state.ContainerOutputLastStateHash != containerOutputStateHash;

            if (!containerInputChanged
                && !containerOutputChanged
                && !force)
            {
                return;
            }

            if (containerOutputChanged)
            {
                // ensure it will be refreshed
                state.CraftingQueue.IsContainerOutputFull = false;
            }

            state.ContainerInputLastStateHash = containerInputStateHash;
            state.ContainerOutputLastStateHash = containerOutputStateHash;

            RefreshRecipe(state, config, objectManufacturer);
        }

        /// <summary>
        /// Refreshes best matching recipe, auto-select it if needed by config.
        /// </summary>
        private static void RefreshRecipe(
            ManufacturingState state,
            ManufacturingConfig config,
            IStaticWorldObject objectManufacturer)
        {
            var selectedRecipe = state.SelectedRecipe;
            var bestRecipe = SharedMatchBestRecipe(state,
                                                   config,
                                                   objectManufacturer);

            if (bestRecipe is not null
                && config.IsAutoSelectRecipe)
            {
                // auto-select the best recipe
                selectedRecipe = state.SelectedRecipe = bestRecipe;
                bestRecipe = null;
                state.CraftingQueue.Clear();
            }

            if (selectedRecipe is null)
            {
                return;
            }

            // refresh selected recipe
            var isSelectedRecipeCanBeCrafted = selectedRecipe.CanBeCrafted(
                objectManufacturer,
                state.CraftingQueue,
                countToCraft: 1);

            if (isSelectedRecipeCanBeCrafted)
            {
                var currentCraftingRecipe = state.CraftingQueue.QueueItems.FirstOrDefault();
                if (currentCraftingRecipe is null
                    || currentCraftingRecipe.Recipe != selectedRecipe)
                {
                    // there is nothing crafting or something different is crafting - start crafting the new selected recipe
                    Logger.Info($"Manufacturing of recipe {selectedRecipe} started at {objectManufacturer}");
                    CraftingMechanics.ServerStartCrafting(
                        objectManufacturer,
                        null,
                        state.CraftingQueue,
                        selectedRecipe,
                        countToCraft: ushort.MaxValue);
                }
            }
            else if (state.CraftingQueue.QueueItems.Count > 0)
            {
                // the selected recipe cannot be crafted
                // clear current queue (progress is lost!)
                // nothing will be crafted now
                Logger.Info($"Manufacturing stopped at {objectManufacturer} - the recipe cannot be crafted anymore");
                state.CraftingQueue.Clear();
            }
        }
    }
}