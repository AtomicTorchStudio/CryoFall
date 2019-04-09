namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.LandClaims.Data
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Construction.Data;

    public class ViewModelProtoLandClaimInfo : BaseViewModel
    {
        private readonly IProtoObjectLandClaim protoObjectLandClaim;

        public ViewModelProtoLandClaimInfo(IProtoObjectLandClaim protoObjectLandClaim)
        {
            this.protoObjectLandClaim = protoObjectLandClaim;
            this.ViewModelStructure = new ViewModelStructure(this.protoObjectLandClaim);
        }

        public int CurrentStructureLandClaimAreaSize
            => this.protoObjectLandClaim.LandClaimSize;

        public string CurrentStructureLandClaimDestructionTimeout
            => ClientTimeFormatHelper.FormatTimeDuration(this.protoObjectLandClaim.DestructionTimeout, 
                                                         trimRemainder: true);

        public int CurrentStructureLevel
        {
            get
            {
                // let's extract the level from the structure class name
                // (assume it's the number in the end of the class name)
                var className = this.protoObjectLandClaim.GetType().Name;
                var result = 0;
                var currentMult = 1;
                var pos = className.Length - 1;
                do
                {
                    if (!char.IsDigit(className, pos))
                    {
                        break;
                    }

                    result += currentMult * (int)char.GetNumericValue(className, pos);

                    pos--;
                    currentMult *= 10;
                }
                while (pos > 0);

                return result;
            }
        }

        public int CurrentStructureSafeItemsSlotsCount
            => this.protoObjectLandClaim.SafeItemsSlotsCount;

        public ViewModelStructure ViewModelStructure { get; }
    }
}