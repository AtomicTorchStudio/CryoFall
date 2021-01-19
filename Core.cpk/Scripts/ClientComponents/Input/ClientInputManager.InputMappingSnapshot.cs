namespace AtomicTorch.CBND.CoreMod.ClientComponents.Input
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.GameEngine.Common.Extensions;

    public static partial class ClientInputManager
    {
        private class InputMappingSnapshot
        {
            private readonly Dictionary<string, ButtonMapping> abstractMapping
                = new(capacity: 256);

            private readonly HashSet<string> knownEnumTypeNames = new();

            public Dictionary<IWrappedButton, ButtonMapping> GetMapping(
                IReadOnlyCollection<Type> buttonEnumTypes,
                IReadOnlyDictionary<IWrappedButton, ButtonInfoAttribute> buttons)
            {
                // map all button enums
                var abstractButtons = buttons.ToDictionary(
                    b => GetButtonId(b.Key),
                    b => b);

                var result = new Dictionary<IWrappedButton, ButtonMapping>();
                foreach (var mapping in this.abstractMapping)
                {
                    if (abstractButtons.TryGetValue(mapping.Key, out var button))
                    {
                        result[button.Key] = mapping.Value;
                    }
                    else
                    {
                        // this button is unknown
                    }
                }

                // set all not found buttons to default values
                foreach (var pair in buttons)
                {
                    if (!result.ContainsKey(pair.Key))
                    {
                        result[pair.Key] = pair.Value.DefaultButtonMapping;
                    }
                }

                return result;
            }

            public void Update(InputMappingSnapshot snapshot)
            {
                // copy all mappings from another snapshot
                foreach (var pair in snapshot.abstractMapping)
                {
                    this.abstractMapping[pair.Key] = pair.Value;
                }

                // copy all enum ids from another snapshot
                this.knownEnumTypeNames.AddRange(snapshot.knownEnumTypeNames);
            }

            public void Update(
                IReadOnlyCollection<Type> knownEnumTypes,
                IReadOnlyDictionary<IWrappedButton, ButtonMapping> mapping)
            {
                this.knownEnumTypeNames.AddRange(knownEnumTypes.Select(t => t.FullName));

                foreach (var pair in mapping)
                {
                    var key = GetButtonId(pair.Key);
                    this.abstractMapping[key] = pair.Value;
                }
            }

            private static string GetButtonId(IWrappedButton wrappedButton)
            {
                return wrappedButton.WrappedButtonType.FullName + "." + wrappedButton.WrappedButtonName;
            }
        }
    }
}