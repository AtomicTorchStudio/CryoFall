namespace AtomicTorch.CBND.CoreMod.Systems.Crafting
{
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class RecipeWithSkin : IRemoteCallParameter
    {
        public readonly IProtoItem ProtoItemSkinOverride;

        public readonly Recipe Recipe;

        public RecipeWithSkin(Recipe recipe, IProtoItem protoItemSkinOverride = null)
        {
            this.Recipe = recipe;
            this.ProtoItemSkinOverride = protoItemSkinOverride;
        }

        public override string ToString()
        {
            return this.ProtoItemSkinOverride is null
                       ? this.Recipe.ShortId
                       : this.Recipe.ShortId + " with skin " + this.ProtoItemSkinOverride.ShortId;
        }

        public void Validate()
        {
            if (this.ProtoItemSkinOverride is not null)
            {
                this.Recipe.ValidateSkin(this.ProtoItemSkinOverride);
            }
        }
    }
}