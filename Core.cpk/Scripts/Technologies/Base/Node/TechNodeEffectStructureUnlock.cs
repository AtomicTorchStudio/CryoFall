namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;
    using AtomicTorch.CBND.GameApi.Resources;

    public class TechNodeEffectStructureUnlock : BaseTechNodeEffect
    {
        public TechNodeEffectStructureUnlock(IProtoObjectStructure structure)
        {
            this.Structure = structure ?? throw new ArgumentNullException();
        }

        public override string Description => "Unlocks structure " + this.Structure.Name;

        public override ITextureResource Icon => this.Structure.Icon;

        public override string ShortDescription => this.Structure.Name;

        public IProtoObjectStructure Structure { get; }

        public override BaseViewModelTechNodeEffect CreateViewModel()
        {
            return new ViewModelTechNodeEffectStructureUnlock(this);
        }

        public override void PrepareEffect(TechNode techNode)
        {
            this.Structure.PrepareProtoSetLinkWithTechNode(techNode);
        }
    }
}