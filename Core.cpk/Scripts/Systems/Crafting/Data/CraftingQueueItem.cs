namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class CraftingQueueItem : BaseNetObject
    {
        public CraftingQueueItem(RecipeWithSkin recipeEntry, ushort countToCraft, ushort localId)
        {
            this.RecipeEntry = recipeEntry;
            this.CountToCraftRemains = countToCraft;
            this.LocalId = localId;
        }

        [SyncToClient]
        public ushort CountToCraftRemains { get; internal set; }

        [SyncToClient]
        public ushort LocalId { get; }

        [SyncToClient]
        public RecipeWithSkin RecipeEntry { get; }

        public bool CanCombineWith(RecipeWithSkin recipeEntry)
        {
            if (recipeEntry.Recipe != this.RecipeEntry.Recipe
                || recipeEntry.ProtoItemSkinOverride is not null
                || this.RecipeEntry.ProtoItemSkinOverride is not null)
            {
                return false;
            }

            foreach (var outputItem in this.RecipeEntry.Recipe.OutputItems.Items)
            {
                if (!outputItem.ProtoItem.IsStackable)
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            return "Craft queue item for " + this.RecipeEntry;
        }
    }
}