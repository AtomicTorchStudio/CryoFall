namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class CraftingQueueItem : BaseNetObject
    {
        public CraftingQueueItem(Recipe recipe, ushort countToCraft, ushort localId)
        {
            this.Recipe = recipe;
            this.CountToCraftRemains = countToCraft;
            this.LocalId = localId;
        }

        [SyncToClient]
        public ushort CountToCraftRemains { get; internal set; }

        [SyncToClient]
        public ushort LocalId { get; }

        [SyncToClient]
        public Recipe Recipe { get; }

        public bool CanCombineWith(Recipe recipe)
        {
            return recipe == this.Recipe
                   && this.Recipe.OutputItems.Items.All(i => i.ProtoItem.IsStackable);
        }

        public override string ToString()
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            return "Craft queue item for " + this.Recipe.ShortId;
        }
    }
}