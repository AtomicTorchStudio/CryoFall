namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Scripting;

    // ReSharper disable all InconsistentNaming
    public static class StructuresHelper
    {
        private static Lazy<IReadOnlyList<IProtoObjectStructure>> allConstructableStructures;

        private static Lazy<IReadOnlyList<IProtoObjectStructure>> allConstructableStructuresIncludingUpgrades;

        private static Lazy<IReadOnlyList<IProtoObjectStructure>> loadingSplashScreenStructures;

        static StructuresHelper()
        {
            TechGroup.AvailableTechGroupsChanged += TechGroupOnAvailableTechGroupsChanged;
            Reinitialize();
        }

        public static IReadOnlyList<IProtoObjectStructure> AllConstructableStructures
            => allConstructableStructures.Value;

        public static IReadOnlyList<IProtoObjectStructure> AllConstructableStructuresIncludingUpgrades
            => allConstructableStructuresIncludingUpgrades.Value;

        public static IReadOnlyList<IProtoObjectStructure> LoadingSplashScreenStructures
            => loadingSplashScreenStructures.Value;

        public static ICollection<IProtoObjectStructure> ClientGetAvailableToBuildStructures()
        {
            var character = Api.Client.Characters.CurrentPlayerCharacter;
            var result = new List<IProtoObjectStructure>(AllConstructableStructures);

            // remove locked structures
            result.RemoveAll(s => !s.SharedIsTechUnlocked(character));

            return result;
        }

        private static void Reinitialize()
        {
            loadingSplashScreenStructures = new(
                () => Api.FindProtoEntities<IProtoObjectStructure>()
                         .Where(s => s.ConfigBuild.IsAllowed
                                     && (s.IsAutoUnlocked
                                         || (s.IsListedInTechNodes
                                             && s.ListedInTechNodes.Any(
                                                 n => n.AvailableIn == FeatureAvailability.All))))
                         .ToArray());

            allConstructableStructuresIncludingUpgrades = new(
                () => Api.FindProtoEntities<IProtoObjectStructure>()
                         .Where(s => s.IsAutoUnlocked
                                     || (s.IsListedInTechNodes
                                         && s.ListedInTechNodes.Any(
                                             n => n.IsAvailable)))
                         .ToArray());

            allConstructableStructures = new(
                () => Api.FindProtoEntities<IProtoObjectStructure>()
                         .Where(s => s.ConfigBuild.IsAllowed
                                     && (s.IsAutoUnlocked
                                         || (s.IsListedInTechNodes
                                             && s.ListedInTechNodes.Any(
                                                 n => n.IsAvailable))))
                         .ToArray());
        }

        private static void TechGroupOnAvailableTechGroupsChanged()
        {
            Reinitialize();
        }
    }
}