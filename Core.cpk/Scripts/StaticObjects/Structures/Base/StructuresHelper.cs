namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class StructuresHelper
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<IReadOnlyList<IProtoObjectStructure>> allConstructableStructures
            = new Lazy<IReadOnlyList<IProtoObjectStructure>>(
                () => Api.FindProtoEntities<IProtoObjectStructure>()
                         .Where(s => s.ConfigBuild.IsAllowed
                                     && (s.IsAutoUnlocked
                                         || s.IsListedInTechNodes))
                         .ToList());

        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<IReadOnlyList<IProtoObjectStructure>> allStructuresIncludingUpgrades
            = new Lazy<IReadOnlyList<IProtoObjectStructure>>(
                () => Api.FindProtoEntities<IProtoObjectStructure>()
                         .Where(s => s.IsAutoUnlocked
                                     || s.IsListedInTechNodes)
                         .ToList());

        public static IReadOnlyList<IProtoObjectStructure> AllConstructableStructures
            => allConstructableStructures.Value;

        public static IReadOnlyList<IProtoObjectStructure> AllStructuresIncludingUpgrades
            => allStructuresIncludingUpgrades.Value;

        public static ICollection<IProtoObjectStructure> ClientGetAvailableToBuildStructures()
        {
            var character = Api.Client.Characters.CurrentPlayerCharacter;
            var result = new List<IProtoObjectStructure>(AllConstructableStructures);

            // remove locked structures
            result.RemoveAll(s => !s.SharedIsTechUnlocked(character));

            return result;
        }
    }
}