namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.CharacterOrigins;

    public class ParameterTypeProtoOrigin : BaseConsoleCommandParameterProtoType<ProtoCharacterOrigin>
    {
        protected override List<ProtoCharacterOrigin> GetProtoEntitiesList()
        {
            return FindProtoEntities<ProtoCharacterOrigin>()
                   .OrderBy(i => i.ShortId, StringComparer.OrdinalIgnoreCase)
                   .ToList();
        }
    }
}