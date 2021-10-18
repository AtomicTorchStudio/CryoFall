namespace AtomicTorch.CBND.CoreMod.ItemContainers
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Drones;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemDisassemblySystem : ProtoSystem<ItemDisassemblySystem>
    {
        /// <summary>
        /// Player can recover from 20% to 50% of the original item's ingredients for an item with 100% durability.
        /// It scales linearly with the item's durability (e.g. for an item with 50% durability player will
        /// receive 10-25% of the ingredients).
        /// Please note: for some rare ingredients (e.g. only a single electronic component) this will serve
        /// as the probability of recovering this ingredient item.
        /// </summary>
        public static readonly (double min, double max) DisassemblyOutputRange = (0.2, 0.5);

        public static void ServerDisassemble(
            ICharacter character,
            IItemsContainer containerInput,
            IItemsContainer containerOutput,
            byte defaultOutputSlotsCount)
        {
            if (containerInput.OccupiedSlotsCount == 0
                || containerOutput.OccupiedSlotsCount > 0)
            {
                throw new InvalidOperationException();
            }

            Server.Items.SetSlotsCount(containerOutput, byte.MaxValue);

            var itemsDisassembled = 0;
            foreach (var item in Api.Shared
                                    .WrapInTempList(containerInput.Items)
                                    .EnumerateAndDispose())
            {
                if (!SharedCanDisassemble(item.ProtoItem))
                {
                    continue;
                }

                Disassemble(item);
            }

            if (itemsDisassembled > 0)
            {
                character.ServerAddSkillExperience<SkillMaintenance>(
                    SkillMaintenance.ExperiencePerItemDisassembled * itemsDisassembled);
            }

            Server.Items.SetSlotsCount(containerOutput,
                                       Math.Max(containerOutput.OccupiedSlotsCount,
                                                defaultOutputSlotsCount));

            void Disassemble(IItem itemToDisassemble)
            {
                var recipe = SharedGetSingleRecipe(itemToDisassemble.ProtoItem);
                if (recipe is null)
                {
                    return;
                }

                // the recipe may produce a stack of items (e.g. 20 ammo)
                // adjust the output count depending on how many items are disassembled
                var recipeCountMultiplier = itemToDisassemble.Count
                                            / (double)recipe.OutputItems.Items[0].Count;

                Server.Items.DestroyItem(itemToDisassemble);
                itemsDisassembled++;

                var range = DisassemblyOutputRange;
                var durabilityFraction = ItemDurabilitySystem.SharedGetDurabilityFraction(itemToDisassemble);
                foreach (var itemToRecover in recipe.InputItems)
                {
                    var outputCountMultipier = range.min + RandomHelper.NextDouble() * (range.max - range.min);
                    var probability = Math.Min(1, durabilityFraction * outputCountMultipier);

                    var countToRecover = recipeCountMultiplier * (double)itemToRecover.Count;
                    countToRecover *= probability;

                    if (countToRecover < 1)
                    {
                        // try to roll at least one item 
                        if (!RandomHelper.RollWithProbability(probability * recipeCountMultiplier))
                        {
                            // cannot recover this item
                            continue;
                        }

                        countToRecover = 1;
                    }

                    Server.Items.CreateItem(itemToRecover.ProtoItem,
                                            containerOutput,
                                            (ushort)countToRecover);
                }
            }
        }

        public static bool SharedCanDisassemble(IProtoItem protoItem)
        {
            if (!SharedIsCompatibleProtoItem(protoItem))
            {
                return false;
            }

            return SharedGetSingleRecipe(protoItem) is not null;
        }

        private static Recipe SharedGetSingleRecipe(IProtoItem protoItem)
        {
            return Recipe.AllRecipes.SingleOrDefault(
                r => r.OutputItems.Count == 1
                     && r.OutputItems.Items[0].ProtoItem == protoItem
                     && r.OutputItems.Items[0].CountRandom == 0
                     // ensure that items such as gold-plated pistol cannot be disassembled
                     // there should be no original ingredients that have a durability/freshness property
                     && r.InputItems.All(
                         i => (i.ProtoItem is not IProtoItemWithDurability protoItemWithDurability
                               || protoItemWithDurability.DurabilityMax == 0)
                              && (i.ProtoItem is not IProtoItemWithFreshness protoItemWithFreshness
                                  || protoItemWithFreshness.FreshnessMaxValue == 0)));
        }

        private static bool SharedIsCompatibleProtoItem(IProtoItem protoItem)
        {
            return protoItem switch
            {
                IProtoItemEquipment    => true,
                IProtoItemTool         => true,
                IProtoItemWeapon       => true,
                IProtoItemAmmo         => true,
                IProtoItemDrone        => true,
                IProtoItemDroneControl => true,
                _                      => false
            };
        }
    }
}