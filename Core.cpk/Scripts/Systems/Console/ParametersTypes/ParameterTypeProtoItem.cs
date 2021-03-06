﻿namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ParameterTypeProtoItem : BaseConsoleCommandParameterProtoType<IProtoItem>
    {
        protected override List<IProtoItem> GetProtoEntitiesList()
        {
            return FindProtoEntities<IProtoItem>()
                   .Where(e => e.Icon is not null)
                   .OrderBy(i => i.ShortId, StringComparer.OrdinalIgnoreCase)
                   .ToList();
        }
    }
}