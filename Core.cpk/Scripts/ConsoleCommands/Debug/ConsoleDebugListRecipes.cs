// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;

    public class ConsoleDebugListRecipes : BaseConsoleCommand
    {
        public enum ListType
        {
            [Description("Auto-unlocked")]
            AutoUnlocked,

            [Description("Unavailable")]
            Unavailable
        }

        public override string Description =>
            "Lists all the recipes and buildings and their status (except for recipes/buildings that already belong to any technology nodes)."
            + " This command is used to find orphaned recipes/buildings.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ClientAndServerOperatorOnly;

        public override string Name => "debug.listRecipes";

        public string Execute(ListType listType)
        {
            switch (listType)
            {
                case ListType.AutoUnlocked:
                    return this.ListAutoUnlocked();

                case ListType.Unavailable:
                    return this.ListUnavailable();

                default:
                    throw new ArgumentOutOfRangeException(nameof(listType), listType, null);
            }
        }

        private void AppendLineRecipe(StringBuilder sb, Recipe recipe)
        {
            sb.Append("* ")
              .Append("(")
              .Append(this.GetRecipeTypeString(recipe))
              .Append(") ")
              .Append(recipe.ShortId)
              .Append(" - ")
              .Append(recipe.Name)
              .AppendLine();
        }

        private void AppendLineStructure(StringBuilder sb, IProtoObjectStructure structure)
        {
            sb.Append("* ")
              .Append(structure.ShortId)
              .Append(" - ")
              .Append(structure.Name)
              .AppendLine();
        }

        private string GetRecipeTypeString(Recipe recipe)
        {
            switch (recipe)
            {
                case Recipe.RecipeForManufacturingByproduct:
                    return "Manufacturing byproduct";

                case Recipe.RecipeForManufacturing:
                    return "Manufacturing";

                case Recipe.RecipeForHandCrafting:
                    return "Craft/Hand";

                case Recipe.RecipeForStationCrafting:
                    return "Craft/Station";

                default:
                    return "[" + recipe.GetType().BaseType.Name + "]";
            }
        }

        private string ListAutoUnlocked()
        {
            var autoUnlockedRecipes = Recipe.AllRecipes
                                            .Where(r => r.IsAutoUnlocked)
                                            .ToList();

            var autoUnlockedStructures = StructuresHelper.AllConstructableStructures
                                                         .Where(s => s.IsAutoUnlocked)
                                                         .ToList();

            return this.Print("Auto-unlocked", autoUnlockedRecipes, autoUnlockedStructures);
        }

        private string ListUnavailable()
        {
            var unavailableRecipes = Recipe.AllRecipes
                                           .Where(r => r.ListedInTechNodes.Count == 0
                                                       && !r.IsAutoUnlocked)
                                           .ToList();

            var unavailableStructures = StructuresHelper.AllConstructableStructures
                                                        .Where(s => s.ListedInTechNodes.Count == 0
                                                                    && !s.IsAutoUnlocked)
                                                        .ToList();

            return this.Print("Unavailable", unavailableRecipes, unavailableStructures);
        }

        private string Print(string messagePrefix, List<Recipe> recipes, List<IProtoObjectStructure> structures)
        {
            var sb = new StringBuilder()
                     .AppendLine()
                     .AppendLine();

            if (recipes.Count > 0)
            {
                sb.Append(messagePrefix);
                sb.Append(" ");
                sb.AppendLine("Recipes:");
                foreach (var recipe in recipes.OrderBy(this.GetRecipeTypeString))
                {
                    this.AppendLineRecipe(sb, recipe);
                }

                sb.AppendLine();
            }
            else
            {
                sb.AppendLine($"<no {messagePrefix.ToLowerInvariant()} item recipes found>");
            }

            if (structures.Count > 0)
            {
                sb.Append(messagePrefix);
                sb.Append(" ");
                sb.AppendLine("Structures:");
                foreach (var structure in structures)
                {
                    this.AppendLineStructure(sb, structure);
                }

                sb.AppendLine();
            }
            else
            {
                sb.AppendLine($"<no {messagePrefix.ToLowerInvariant()} structures found>");
            }

            return sb.ToString();
        }
    }
}