namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    public enum RecipeType : byte
    {
        /// <summary>
        /// Can be crafted from inventory.
        /// </summary>
        Hand,

        /// <summary>
        /// Can only be crafted from a crafting station, but when recipe is added to the player crafting queue
        /// then the player doesn't have to stand next to the station and can leave.
        /// When the recipe finishes crafting the item will appear in the player inventory.
        /// </summary>
        StationCrafting,

        /// <summary>
        /// Items are crafted in factories by the factories themselves.
        /// Players put items into the factory storage slot and select a recipe to be crafted.
        /// Then that factory crafts this selected item.
        /// Requires researched recipe to put that recipe into production (same as with other crafting).
        /// </summary>
        Manufacturing,

        /// <summary>
        /// TODO: add description
        /// </summary>
        ManufacturingByproduct
    }
}