namespace AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Data;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelRecipeBreakdown : BaseViewModel
    {
        public static readonly HashSet<Type> BasicItems
            = new()
            {
                typeof(ItemFibers),
                typeof(ItemPlanks),
                typeof(ItemCharcoal),
                typeof(ItemPaper),
                typeof(ItemIngotIron),
                typeof(ItemIngotCopper),
                typeof(ItemIngotGold),
                typeof(ItemCoinPenny),
                typeof(ItemCoinShiny),
                typeof(ItemBottleEmpty),
                typeof(ItemBottleWater),
                typeof(ItemGlassRaw),
                typeof(ItemGlue),
                typeof(ItemToxin),
                typeof(ItemSand),
                typeof(ItemSalt),
                typeof(ItemRiceCooked),
                typeof(ItemDough),
                typeof(ItemMeatRoasted),
                typeof(ItemAsh),
                typeof(ItemRubberRaw),
                typeof(ItemCanisterEmpty),
                typeof(ItemCanisterGasoline),
                typeof(ItemCanisterMineralOil),
                typeof(ItemCanisterPetroleum)
            };

        private readonly string recipeTimeCalculation;

        public ViewModelRecipeBreakdown(Recipe recipe)
        {
            var inputItems = new List<ProtoItemWithCountFractional>();
            var outputItems = new List<ProtoItemWithCountFractional>();
            var outputItemsExtras = new List<ProtoItemWithCountFractional>();
            var intermediateRecipes = new List<ViewModelIntermediateRecipe>();
            var inputNutrition = new FoodNutritionValueData();
            var outputNutrition = new FoodNutritionValueData();

            var multiplier = 1 / (double)recipe.OutputItems.Items[0].Count;
            var isOriginalRecipeManufacturing = recipe.RecipeType == RecipeType.Manufacturing
                                                || recipe.RecipeType == RecipeType.ManufacturingByproduct;

            var originalRecipeDurationPerItem = recipe.OriginalDuration * multiplier;
            double specificDurationCrafting,
                   specificDurationManufacturing;

            if (isOriginalRecipeManufacturing)
            {
                specificDurationCrafting = 0;
                specificDurationManufacturing = originalRecipeDurationPerItem;
            }
            else
            {
                specificDurationCrafting = originalRecipeDurationPerItem;
                specificDurationManufacturing = 0;
            }

            var timeSb = new StringBuilder()
                         .AppendLine($"{recipe.Name} — per single item:")
                         .AppendLine();
            var recursiveErrorsSb = new StringBuilder();
            var tag = isOriginalRecipeManufacturing ? "[manufacture]" : "[craft]";
            timeSb.AppendLine($"{tag} {originalRecipeDurationPerItem:0.##}s — x{multiplier:0.##} {recipe.Name}");

            foreach (var outputItem in recipe.OutputItems.Items)
            {
                var entry = new ProtoItemWithCountFractional(outputItem.ProtoItem,
                                                             outputItem.Count * multiplier);
                outputItems.Add(entry);
                outputNutrition.TryAdd(entry.ProtoItem, entry.Count);
            }

            ProcessInputRecursive(recipe, multiplier, depth: 0);

            inputItems.Sort(ListOrderComparison);
            outputItems.Sort(ListOrderComparison);
            outputItemsExtras.Sort(ListOrderComparison);

            this.InputItems = inputItems;
            this.OutputItems = outputItems;
            this.OutputItemsExtras = outputItemsExtras;
            this.IntermediateRecipes = intermediateRecipes;

            this.InputTotalFoodNutritionValue = inputNutrition;
            this.OutputTotalFoodNutritionValue = outputNutrition;

            this.RecipesDurationTotalText = ClientTimeFormatHelper.FormatTimeDuration(
                                                specificDurationCrafting,
                                                roundSeconds: false)
                                            + " + "
                                            + ClientTimeFormatHelper.FormatTimeDuration(
                                                specificDurationManufacturing,
                                                roundSeconds: false);

            timeSb.AppendLine()
                  .AppendLine($"Total duration: {this.RecipesDurationTotalText} (crafting + manufacturing)");
            this.recipeTimeCalculation = timeSb.ToString().Trim('\r', '\n');

            this.RecursiveErrors = recursiveErrorsSb.Length > 0
                                       ? "Nested recipes recursion detected."
                                         + Environment.NewLine
                                         + "Recursion was broken when looking for recipes for these item(s):"
                                         + Environment.NewLine
                                         + recursiveErrorsSb
                                         + Environment.NewLine
                                         + "Intermediate recipes list:"
                                         + string.Join("",
                                                       this.IntermediateRecipes.Select(
                                                           r => Environment.NewLine
                                                                + "* "
                                                                + r.ViewModelCraftingRecipe.Title))
                                         + Environment.NewLine
                                         + Environment.NewLine
                                         + $"Please edit {nameof(BasicItems)} or {nameof(RecipesHelper.BlacklistRecipes)} lists in {nameof(ViewModelRecipeBreakdown)}.cs (Editor.mpk)"
                                       : null;

            void ProcessInputRecursive(Recipe outerRecipe, double outerMultiplier, int depth)
            {
                foreach (var inputItem in outerRecipe.InputItems)
                {
                    if (BasicItems.Contains(inputItem.ProtoItem.GetType()))
                    {
                        // don't lookup recipe
                        var entry = new ProtoItemWithCountFractional(inputItem.ProtoItem,
                                                                     inputItem.Count * outerMultiplier);
                        AddListEntry(inputItems, entry);
                        inputNutrition.TryAdd(entry.ProtoItem, entry.Count);
                        continue;
                    }

                    var inputItemRecipe = RecipesHelper.FindFirstRecipe(inputItem.ProtoItem);
                    if (inputItemRecipe is null)
                    {
                        // no recipe
                        var entry = new ProtoItemWithCountFractional(inputItem.ProtoItem,
                                                                     inputItem.Count * outerMultiplier);
                        AddListEntry(inputItems, entry);
                        inputNutrition.TryAdd(entry.ProtoItem, entry.Count);
                        continue;
                    }

                    if (depth > 20)
                    {
                        // recursion went too deep, stop here
                        recursiveErrorsSb.AppendLine("* " + inputItem.ProtoItem.Name);
                        return;
                    }

                    // expand recipe
                    var recipeMultiplier = inputItem.Count / (double)inputItemRecipe.OutputItems.Items[0].Count;
                    AddIntermediateRecipe(intermediateRecipes,
                                          inputItemRecipe,
                                          recipeMultiplier * outerMultiplier);

                    ProcessInputRecursive(inputItemRecipe,
                                          recipeMultiplier * outerMultiplier,
                                          depth: depth + 1);

                    var isManufacturing = inputItemRecipe.RecipeType == RecipeType.Manufacturing
                                          || inputItemRecipe.RecipeType == RecipeType.ManufacturingByproduct;
                    var inputItemRecipeDuration = inputItemRecipe.OriginalDuration
                                                  * recipeMultiplier
                                                  * outerMultiplier;

                    if (isManufacturing)
                    {
                        specificDurationManufacturing += inputItemRecipeDuration;
                    }
                    else
                    {
                        specificDurationCrafting += inputItemRecipeDuration;
                    }

                    var tag = isManufacturing ? "[manufacture]" : "[craft]";
                    timeSb.AppendLine(
                        $"{tag} {ClientTimeFormatHelper.FormatTimeDuration(inputItemRecipeDuration, roundSeconds: true)} — x{recipeMultiplier * outerMultiplier:0.##} {inputItemRecipe.Name} (for {outerRecipe.Name})");

                    foreach (var outputItem in inputItemRecipe.OutputItems.Items)
                    {
                        if (outputItem.ProtoItem == inputItem.ProtoItem)
                        {
                            continue;
                        }

                        var entry = new ProtoItemWithCountFractional(outputItem.ProtoItem,
                                                                     outputItem.Count
                                                                     * recipeMultiplier
                                                                     * outerMultiplier);
                        AddListEntry(outputItemsExtras, entry);
                        outputNutrition.TryAdd(entry.ProtoItem, entry.Count);
                    }
                }
            }
        }

        public BaseCommand CommandDisplayTimeCalculation =>
            new ActionCommand(this.ExecuteCommandDisplayTimeCalculation);

        public IReadOnlyList<ProtoItemWithCountFractional> InputItems { get; }

        public FoodNutritionValueData InputTotalFoodNutritionValue { get; }

        public IReadOnlyList<ViewModelIntermediateRecipe> IntermediateRecipes { get; }

        public IReadOnlyList<ProtoItemWithCountFractional> OutputItems { get; }

        public IReadOnlyList<ProtoItemWithCountFractional> OutputItemsExtras { get; }

        public FoodNutritionValueData OutputTotalFoodNutritionValue { get; }

        public string RecipesDurationTotalText { get; }

        public string RecursiveErrors { get; }

        private static void AddIntermediateRecipe(
            List<ViewModelIntermediateRecipe> recipes,
            Recipe recipe,
            double multiplier)
        {
            Api.Assert(multiplier > 0, "Multiplier should be > 0");

            foreach (var viewModel in recipes)
            {
                if (!ReferenceEquals(viewModel.ViewModelCraftingRecipe.Recipe, recipe))
                {
                    continue;
                }

                viewModel.Multiplier += multiplier;
                return;
            }

            recipes.Add(new ViewModelIntermediateRecipe(recipe, multiplier));
        }

        private static void AddListEntry(
            List<ProtoItemWithCountFractional> list,
            ProtoItemWithCountFractional entry)
        {
            for (var index = 0; index < list.Count; index++)
            {
                var existingEntry = list[index];
                if (!ReferenceEquals(existingEntry.ProtoItem, entry.ProtoItem))
                {
                    continue;
                }

                // entry found - combine them
                list[index] = new ProtoItemWithCountFractional(entry.ProtoItem,
                                                               entry.Count + existingEntry.Count);
                return;
            }

            // entry not found - add new entry
            list.Add(entry);
        }

        private static int ListOrderComparison(ProtoItemWithCountFractional x, ProtoItemWithCountFractional y)
        {
            return string.Compare(x.ProtoItem.Id, y.ProtoItem.Id, StringComparison.Ordinal);
        }

        private void ExecuteCommandDisplayTimeCalculation()
        {
            var dialog = DialogWindow.ShowDialog("Recipe time breakdown",
                                                 new TextBox
                                                 {
                                                     Text = this.recipeTimeCalculation,
                                                     TextWrapping = TextWrapping.Wrap,
                                                     TextAlignment = TextAlignment.Left,
                                                     BorderThickness = default,
                                                     Background = null,
                                                     IsReadOnly = true
                                                 },
                                                 closeByEscapeKey: true,
                                                 focusOnCancelButton: true);
            dialog.GameWindow.Width = 550;
        }
    }
}