namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using System;
    using AtomicTorch.CBND.CoreMod.Perks.Base;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;
    using AtomicTorch.CBND.GameApi.Resources;

    public class TechNodeEffectPerkUnlock : BaseTechNodeEffect
    {
        public TechNodeEffectPerkUnlock(ProtoPerk perk)
        {
            this.Perk = perk ?? throw new ArgumentNullException();
        }

        public override string Description => "Unlocks perk " + this.Perk.Name;

        public override ITextureResource Icon => this.Perk.Icon;

        public ProtoPerk Perk { get; }

        public override string ShortDescription => this.Perk.Name;

        public override BaseViewModelTechNodeEffect CreateViewModel()
        {
            return new ViewModelTechNodeEffectPerkUnlock(this);
        }

        public override void PrepareEffect(TechNode techNode)
        {
            this.Perk.PrepareProtoSetLinkWithTechNode(techNode);
        }
    }
}