namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ParameterTypeProtoStaticWorldObject : BaseConsoleCommandParameterProtoType<IProtoStaticWorldObject>
    {
        protected override List<IProtoStaticWorldObject> GetProtoEntitiesList()
        {
            return FindProtoEntities<IProtoStaticWorldObject>()
                   .Where(e => !(e is IProtoObjectLandClaim))
                   .OrderBy(i => i.ShortId, StringComparer.OrdinalIgnoreCase)
                   .ToList();
        }
    }
}