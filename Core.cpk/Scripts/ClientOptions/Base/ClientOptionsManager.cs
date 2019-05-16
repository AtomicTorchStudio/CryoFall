namespace AtomicTorch.CBND.CoreMod.ClientOptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientOptionsManager
    {
        public static List<IProtoOption> Options { get; private set; }

        public static TOption GetOption<TOption>()
            where TOption : IProtoOption, new()
        {
            var type = typeof(TOption);
            foreach (var option in Options)
            {
                if (option.GetType() == type)
                {
                    return (TOption)option;
                }
            }

            throw new Exception("Option instance not found: " + type.FullName);
        }

        public static IEnumerable<IProtoOption> GetOptionsForCategory(ProtoOptionsCategory category)
        {
            foreach (var option in Options)
            {
                if (option.Category == category)
                {
                    yield return option;
                }
            }
        }

        public static void Initialize()
        {
            if (Options != null)
            {
                return;
            }

            Options = Api.Shared.FindScriptingTypes<IProtoOption>()
                         .Select(t => t.CreateInstance())
                         .ToList();
        }
    }
}