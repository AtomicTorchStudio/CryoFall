namespace AtomicTorch.CBND.CoreMod.Technologies
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class TechGroupRequirementGroupUnlocked<TTechGroup> : BaseTechGroupRequirementGroupUnlocked
        where TTechGroup : TechGroup, new()
    {
        public TechGroupRequirementGroupUnlocked(double completion)
            : base(completion, Api.GetProtoEntity<TTechGroup>())
        {
        }

        public override BaseViewModelTechGroupRequirement CreateViewModel()
        {
            return new ViewModelTechGroupRequirementGroupUnlocked(this);
        }
    }
}