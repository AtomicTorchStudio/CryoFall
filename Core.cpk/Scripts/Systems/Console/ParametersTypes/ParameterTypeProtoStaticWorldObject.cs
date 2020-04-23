namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.World;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeProtoStaticWorldObject : BaseConsoleCommandParameterProtoType<IProtoStaticWorldObject>
    {
        public override string ShortDescription => "static world object prototype name";

        protected override List<IProtoStaticWorldObject> GetProtoEntitiesList()
        {
            return FindProtoEntities<IProtoStaticWorldObject>()
                   .Where(e => !(e is IProtoObjectLandClaim))
                   .OrderBy(i => i.ShortId, StringComparer.OrdinalIgnoreCase)
                   .ToList();
        }
    }
}