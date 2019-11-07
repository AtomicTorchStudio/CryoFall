namespace AtomicTorch.CBND.CoreMod.Editor.Controls.RecipeBreakdown.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.CraftRecipes;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelRecipeBreakdown : BaseViewModel
    {
        private static readonly IReadOnlyList<Recipe> AvailableRecipes;

        // don't perform recipe lookup for such basic items
        private static readonly HashSet<Type> BasicItems
            = new HashSet<Type>()
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
                typeof(ItemCanisterEmpty),
                typeof(ItemCanisterGasoline),
                typeof(ItemCanisterMineralOil),
                typeof(ItemCanisterPetroleum)
            };

        private static readonly HashSet<Type> BlacklistRecipes
            = new HashSet<Type>()
            {
                typeof(RecipeFibersFromPlastic),
                typeof(RecipeFibersFromLeaf),
                typeof(RecipeBreakBottle),
                typeof(RecipeCoinPennyRecycle),
                typeof(RecipeCoinShinyRecycle)
            };

        private readonly string recipeTimeCalculation;

        static ViewModelRecipeBreakdown()
        {
            var availableRecipes = new List<Recipe>();
            foreach (var recipe in Api.FindProtoEntities<Recipe>())
            {
                if (!recipe.IsEnabled
                    || recipe.InputItems.Count == 0
                    || recipe.OutputItems.Count == 0)
                {
                    continue;
                }

                var recipeType = recipe.GetType();
                if (BlacklistRecipes.Contains(recipeType))
                {
                    continue;
                }

                switch (recipe.RecipeType)
                {
                    case RecipeType.Hand:
                    case RecipeType.StationCrafting:
                        availableRecipes.Add(recipe);
                        continue;

                    case RecipeType.Manufacturing:
                        availableRecipes.Add(recipe);
                        continue;

                    case RecipeType.ManufacturingByproduct:
                        // ignore
                        continue;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            AvailableRecipes = availableRecipes;
        }

        public ViewModelRecipeBreakdown(Recipe recipe)
        {
            var inputItems = new List<ProtoItemWithCountFractional>();
            var outputItems = new List<ProtoItemWithCountFractional>();
            var outputItemsExtras = new List<ProtoItemWithCountFractional>();
            var intermediateRecipes = new List<ViewModelIntermediateRecipe>();
            var inputNutrition = new NutritionValueInfo();
            var outputNutrition = new NutritionValueInfo();

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
            var tag = isOriginalRecipeManufacturing  ? "[manufacture]" : "[craft]";
            timeSb.AppendLine($"{tag} {originalRecipeDurationPerItem:0.##}s — x{multiplier:0.##} {recipe.Name}");

            foreach (var outputItem in recipe.OutputItems.Items)
            {
                var entry = new ProtoItemWithCountFractional(outputItem.ProtoItem,
                                                             outputItem.Count * multiplier);
                outputItems.Add(entry);
                outputNutrition.TryAdd(entry);
            }

            ProcessInputRecursive(recipe, multiplier, depth: 0);

            this.InputItems = inputItems;
            this.OutputItems = outputItems;
            this.OutputItemsExtras = outputItemsExtras;
            this.IntermediateRecipes = intermediateRecipes;

            this.InputTotalNutritionValue = inputNutrition;
            this.OutputTotalNutritionValue = outputNutrition;

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
                                         + $"Please edit {nameof(BasicItems)} or {nameof(BlacklistRecipes)} lists in {nameof(ViewModelRecipeBreakdown)}.cs (Editor.mpk)"
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
                        inputNutrition.TryAdd(entry);
                        continue;
                    }

                    var inputItemRecipe = FindRecipe(inputItem.ProtoItem);
                    if (inputItemRecipe == null)
                    {
                        // no recipe
                        var entry = new ProtoItemWithCountFractional(inputItem.ProtoItem,
                                                                     inputItem.Count * outerMultiplier);
                        AddListEntry(inputItems, entry);
                        inputNutrition.TryAdd(entry);
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
                        outputNutrition.TryAdd(entry);
                    }
                }
            }
        }

        public BaseCommand CommandDisplayTimeCalculation =>
            new ActionCommand(this.ExecuteCommandDisplayTimeCalculation);

        public IReadOnlyList<ProtoItemWithCountFractional> InputItems { get; }

        public NutritionValueInfo InputTotalNutritionValue { get; }

        public IReadOnlyList<ViewModelIntermediateRecipe> IntermediateRecipes { get; }

        public IReadOnlyList<ProtoItemWithCountFractional> OutputItems { get; }

        public IReadOnlyList<ProtoItemWithCountFractional> OutputItemsExtras { get; }

        public NutritionValueInfo OutputTotalNutritionValue { get; }

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

        private static Recipe FindRecipe(IProtoItem protoItem)
        {
            foreach (var availableRecipe in AvailableRecipes)
            {
                foreach (var outputItem in availableRecipe.OutputItems.Items)
                {
                    if (ReferenceEquals(outputItem.ProtoItem, protoItem))
                    {
                        // found a recipe
                        return availableRecipe;
                    }
                }
            }

            return null;
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