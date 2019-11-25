namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ProtoObjectLandClaimHelper
    {
        private static readonly List<IProtoObjectLandClaim> AllLandClaimsPrototypes;

        static ProtoObjectLandClaimHelper()
        {
            AllLandClaimsPrototypes = Api.FindProtoEntities<IProtoObjectLandClaim>();
        }

        public static IProtoObjectLandClaim GetProtoByTier(byte tier)
        {
            foreach (var landClaim in AllLandClaimsPrototypes)
            {
                if (landClaim.LandClaimTier == tier)
                {
                    return landClaim;
                }
            }

            throw new Exception("Unknown land claim tier: " + tier);
        }
    }
}