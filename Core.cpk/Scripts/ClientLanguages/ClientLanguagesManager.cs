namespace AtomicTorch.CBND.CoreMod.ClientLanguages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Language;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ClientLanguagesManager
    {
        private static ProtoLanguageDefinition currentLanguageDefinition;

        private static IReadOnlyDictionary<string, ProtoLanguageDefinition> languageDefinitions;

        public static event Action CurrentLanguageDefinitionChanged;

        public static IEnumerable<ProtoLanguageDefinition> AllLanguageDefinitions
            => languageDefinitions.Values
                                  .OrderBy(l => l.LanguageTag, StringComparer.OrdinalIgnoreCase);

        public static ProtoLanguageDefinition CurrentLanguageDefinition
        {
            get => currentLanguageDefinition;
            set
            {
                if (value == null)
                {
                    throw new NullReferenceException("Current language definition cannot be null");
                }

                if (currentLanguageDefinition == value
                    && !Api.Shared.IsRequiresLanguageSelection)
                {
                    return;
                }

                currentLanguageDefinition = value;
                Api.Shared.LocalizationLanguageTags = value.LanguageTagWithFallbackLanguages;

                Api.SafeInvoke(() => CurrentLanguageDefinitionChanged?.Invoke());
            }
        }

        public static ProtoLanguageDefinition GetLanguage(string languageTag)
        {
            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            const string defaultLanguage = "en_us";
            if (string.IsNullOrEmpty(languageTag))
            {
                languageTag = defaultLanguage;
            }

            if (languageDefinitions.TryGetValue(languageTag, out var languageDefinition))
            {
                return languageDefinition;
            }

            // the language definition is not found, try to fallback to default language
            if (languageTag == defaultLanguage)
            {
                // should be impossible
                throw new Exception($@"Cannot find language definition for default ({languageTag}) language");
            }

            languageTag = defaultLanguage;
            if (languageDefinitions.TryGetValue(languageTag, out languageDefinition))
            {
                return languageDefinition;
            }

            throw new Exception($@"Cannot find language definition for {languageTag} language");
        }

        [PrepareOrder(afterType: typeof(BootstrapperClientCoreUI))]
        private class BootstrapperLanguages : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                var allLanguages = Api.FindProtoEntities<ProtoLanguageDefinition>();
                var languageDefinitions = new Dictionary<string, ProtoLanguageDefinition>(capacity: allLanguages.Count);
                foreach (var protoLanguage in allLanguages)
                {
                    if (!protoLanguage.IsEnabled)
                    {
                        continue;
                    }

                    try
                    {
                        languageDefinitions.Add(protoLanguage.LanguageTag, protoLanguage);
                    }
                    catch
                    {
                        Logger.Error(
                            $@"The language definition class is already exist for ""{protoLanguage.LanguageTag}""");
                    }
                }

                ClientLanguagesManager.languageDefinitions = languageDefinitions;

                var currentLanguageTag = Api.Shared.LocalizationLanguageTags.Last();
                currentLanguageDefinition = GetLanguage(currentLanguageTag);

                if (!Api.Shared.LocalizationLanguageTags.SequenceEqual(
                        currentLanguageDefinition.LanguageTagWithFallbackLanguages))
                {
                    // fallback languages don't match
                    Api.Shared.LocalizationLanguageTags = currentLanguageDefinition.LanguageTagWithFallbackLanguages;
                }

                if (Api.Shared.IsRequiresLanguageSelection)
                {
                    Logger.Important("The game requires language selection");
                    MenuLanguageSelection.IsDisplayed = true;
                }
            }
        }
    }
}