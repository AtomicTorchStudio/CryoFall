namespace AtomicTorch.CBND.CoreMod.Perks.Base
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Extensions;

    public abstract class ProtoPerk : ProtoEntity, IProtoPerk
    {
        private List<TechNode> listedInTechNodes;

        public abstract ITextureResource Icon { get; }

        public IReadOnlyStatsDictionary ProtoEffects { get; private set; }

        public void PrepareProtoSetLinkWithTechNode(TechNode techNode)
        {
            this.listedInTechNodes ??= new List<TechNode>();
            this.listedInTechNodes.AddIfNotContains(techNode);
        }

        public bool SharedIsPerkUnlocked(ICharacter character, bool allowIfAdmin = true)
        {
            if (this.listedInTechNodes == null
                || this.listedInTechNodes.Count == 0)
            {
                // not unlockable perk!
                return false;
            }

            if (allowIfAdmin
                && CreativeModeSystem.SharedIsInCreativeMode(character))
            {
                return true;
            }

            var techs = character.SharedGetTechnologies();
            foreach (var node in this.listedInTechNodes)
            {
                if (techs.SharedIsNodeUnlocked(node))
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void PrepareEffects(Effects effects)
        {
        }

        protected sealed override void PrepareProto()
        {
            var effects = new Effects();
            this.PrepareEffects(effects);
            this.ProtoEffects = effects.ToReadOnly();

            this.PrepareProtoPerk();
        }

        protected virtual void PrepareProtoPerk()
        {
        }
    }
}