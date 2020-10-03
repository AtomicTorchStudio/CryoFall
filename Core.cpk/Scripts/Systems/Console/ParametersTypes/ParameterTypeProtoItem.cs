namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Items;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeProtoItem : BaseConsoleCommandParameterProtoType<IProtoItem>
    {
        public override string ShortDescription => "proto item name";

        protected override List<IProtoItem> GetProtoEntitiesList()
        {
            return FindProtoEntities<IProtoItem>()
                   .Where(e => e.Icon is not null)
                   .OrderBy(i => i.ShortId, StringComparer.OrdinalIgnoreCase)
                   .ToList();
        }
    }
}