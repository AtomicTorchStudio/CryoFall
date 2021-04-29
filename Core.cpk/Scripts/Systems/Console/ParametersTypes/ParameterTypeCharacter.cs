namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ParameterTypeCharacter : BaseConsoleCommandParameterType
    {
        public override Type ParameterType { get; } = typeof(ICharacter);

        public static IEnumerable<string> GetNameSuggestionsNonStrict(string startsWith)
        {
            var isFound = false;
            foreach (var c in GetAllCharacters())
            {
                if (c.Name.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase))
                {
                    if (c.Name.Equals(startsWith, StringComparison.OrdinalIgnoreCase))
                    {
                        isFound = true;
                    }

                    yield return c.Name;
                }
            }

            if (isFound
                || string.IsNullOrWhiteSpace(startsWith))
            {
                yield break;
            }

            // return the string itself as one of the suggestions
            yield return startsWith;
        }

        public override IEnumerable<string> GetSuggestions()
        {
            return GetAllCharacters().Select(p => p.Name);
        }

        public override bool TryParse(string value, out object result)
        {
            result = GetAllCharacters()
                .FirstOrDefault(c => c.Name.Equals(value, StringComparison.OrdinalIgnoreCase));
            return result is not null;
        }

        private static IEnumerable<ICharacter> GetAllCharacters()
        {
            if (IsServer)
            {
                return Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: false)
                             .OrderBy(c => c.Name);
            }

            using var tempPlayerCharacters = Api.Shared.GetTempList<ICharacter>();
            Client.Characters.GetKnownPlayerCharacters(tempPlayerCharacters);
            return tempPlayerCharacters.AsList()
                                       .OrderBy(c => c.Name);
        }
    }
}