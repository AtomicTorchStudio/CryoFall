namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data;

    /// <summary>
    /// Base class for console generic Proto Entity parameter.
    /// </summary>
    /// <typeparam name="TProto">Type of proto entity (such as IProtoItem, IProtoStructure, etc).</typeparam>
    public abstract class BaseConsoleCommandParameterProtoType<TProto> : BaseConsoleCommandParameterType
        where TProto : class, IProtoEntity
    {
        private static List<TProto> protoEntities;

        private static IReadOnlyList<string> protoEntitiesShortIds;

        public override Type ParameterType { get; } = typeof(TProto);

        public override IEnumerable<string> GetSuggestions()
        {
            return this.GetProtoEntitiesShortIds();
        }

        public override bool TryParse(string value, out object result)
        {
            result = this.GetProtoEntities()
                         .FirstOrDefault(pi => pi.ShortId.Equals(value, StringComparison.OrdinalIgnoreCase));
            return result != null;
        }

        protected IReadOnlyList<TProto> GetProtoEntities()
        {
            return protoEntities ?? (protoEntities = this.GetProtoEntitiesList());
        }

        protected virtual List<TProto> GetProtoEntitiesList()
        {
            return FindProtoEntities<TProto>()
                   .OrderBy(i => i.ShortId, StringComparer.OrdinalIgnoreCase)
                   .ToList();
        }

        protected IReadOnlyList<string> GetProtoEntitiesShortIds()
        {
            if (protoEntitiesShortIds == null)
            {
                protoEntitiesShortIds = this.GetProtoEntities().Select(p => p.ShortId).ToList();
            }

            return protoEntitiesShortIds;
        }
    }
}