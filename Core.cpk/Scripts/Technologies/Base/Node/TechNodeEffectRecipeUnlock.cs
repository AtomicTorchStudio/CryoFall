namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using System;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;
    using AtomicTorch.CBND.GameApi.Resources;

    public class TechNodeEffectRecipeUnlock : BaseTechNodeEffect
    {
        public TechNodeEffectRecipeUnlock(Recipe recipe)
        {
            this.Recipe = recipe ?? throw new ArgumentNullException();
        }

        public override string Description => "Unlocks recipe " + this.Recipe.Name;

        public override ITextureResource Icon => this.Recipe.Icon;

        public Recipe Recipe { get; }

        public override string ShortDescription => this.Recipe.Name;

        public override BaseViewModelTechNodeEffect CreateViewModel()
        {
            return new ViewModelTechNodeEffectRecipeUnlock(this);
        }

        public override void PrepareEffect(TechNode techNode)
        {
            this.Recipe.PrepareProtoSetLinkWithTechNode(techNode);
        }
    }
}