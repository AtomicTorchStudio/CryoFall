namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Extensions;
    using JetBrains.Annotations;

    public class DataEntryRequiredItemProto
    {
        public static readonly HashSet<Type> BasicItems
            = new HashSet<Type>()
            {
                typeof(ItemFibers),
                typeof(ItemPlanks),
                typeof(ItemCharcoal),
                typeof(ItemGlassRaw),
                typeof(ItemToxin),
                typeof(ItemSand),
                typeof(ItemSalt),
                typeof(ItemAsh),
                typeof(ItemRubberRaw)
            };

        private readonly bool hasNextEntry;

        private readonly IProtoItem protoItem;

        private readonly TechNode techNode;

        public DataEntryRequiredItemProto(
            IProtoItem protoItem,
            [CanBeNull] TechNode techNode,
            bool hasNextEntry)
        {
            this.protoItem = protoItem;
            this.techNode = techNode;
            this.hasNextEntry = hasNextEntry;
        }

        public bool IsTechAvailable => this.CheckIsTechAvailable(out _);

        public string Text
        {
            get
            {
                var text = "\u2022\u00A0"; // bullet point
                text += this.protoItem.Name.Replace(" ", "\u00A0");

                this.CheckIsTechAvailable(out var techNodeUnlockRequired);
                if (techNodeUnlockRequired != null)
                {
                    var groupName = techNodeUnlockRequired.Group.NameWithTierName;
                    // replace spaces with non-breaking space char
                    groupName = groupName.Replace(" ", "\u00A0");
                    text += " —\u00A0" + groupName;
                }

                if (this.hasNextEntry)
                {
                    text += Environment.NewLine;
                }

                return text;
            }
        }

        private bool CheckIsTechAvailable(out TechNode techNodeUnlockRequired)
        {
            if (this.IsRecipeDefinedInTheCurrentTechNode())
            {
                techNodeUnlockRequired = null;
                return true;
            }

            var character = ClientCurrentCharacterHelper.Character;
            //if (CreativeModeSystem.SharedIsInCreativeMode(character))
            //{
            //    techNodeUnlockRequired = null;
            //    return true;
            //}

            if (this.protoItem is IProtoItemWithReferenceTech protoItemWithDefaultRecipe
                && protoItemWithDefaultRecipe.ReferenceTech != null)
            {
                var techs = character.SharedGetTechnologies();
                var referenceTech = protoItemWithDefaultRecipe.ReferenceTech;
                if (techs.SharedIsNodeUnlocked(referenceTech))
                {
                    techNodeUnlockRequired = null;
                    return true;
                }

                techNodeUnlockRequired = protoItemWithDefaultRecipe.ReferenceTech;
                return false;
            }

            if (BasicItems.Contains(this.protoItem.GetType()))
            {
                techNodeUnlockRequired = null;
                return true;
            }

            techNodeUnlockRequired = null;
            foreach (var availableRecipe in RecipesHelper.AvailableRecipes)
            {
                foreach (var outputItem in availableRecipe.OutputItems.Items)
                {
                    if (!ReferenceEquals(outputItem.ProtoItem, this.protoItem))
                    {
                        continue;
                    }

                    // found a recipe
                    if (availableRecipe.SharedIsTechUnlocked(character))
                    {
                        techNodeUnlockRequired = null;
                        return true;
                    }

                    if (availableRecipe.ListedInTechNodes.Count == 0)
                    {
                        continue;
                    }

                    // the recipe is locked
                    var minTechNode = availableRecipe.ListedInTechNodes
                                                     .MinimumOrDefault(t => (byte)t.Group.Tier);

                    if (techNodeUnlockRequired is null
                        || minTechNode.Group.Tier < techNodeUnlockRequired.Group.Tier)
                    {
                        // remember this tech group as it's required

                        techNodeUnlockRequired = minTechNode;
                    }
                }
            }

            return techNodeUnlockRequired is null;
        }

        private bool IsRecipeDefinedInTheCurrentTechNode()
        {
            if (this.techNode is null)
            {
                return false;
            }

            foreach (var effect in this.techNode.NodeEffects)
            {
                if (!(effect is TechNodeEffectRecipeUnlock recipeUnlock))
                {
                    continue;
                }

                foreach (var outputItem in recipeUnlock.Recipe.OutputItems.Items)
                {
                    if (ReferenceEquals(outputItem.ProtoItem, this.protoItem))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}