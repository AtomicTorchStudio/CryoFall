namespace AtomicTorch.CBND.CoreMod.ClientLanguages
{
    using System.Linq;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Extensions;
    using JetBrains.Annotations;

    [UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
    public abstract class ProtoLanguageDefinition
    {
        public abstract string AcceptText { get; }

        public string Description =>
            this.NameEnglish == this.NameNative
                ? this.NameEnglish
                : $"{this.NameNative} ({this.NameEnglish})";

        /// <summary>
        /// This property allows to specify an array of languages
        /// which the game will use (from latter to first) to provide the localized string
        /// in case the current language doesn't have a translation.
        /// For example, if you specify { "en_us", "ru_ru" } then the game will try to look for a localized string
        /// for the current language definition, then for "ru_ru", then if it's still missing - in "en_us".
        /// </summary>
        public virtual string[] FallbackLanguages { get; }

        public virtual TextureResource Icon => new TextureResource("Languages/" + this.LanguageTag);

        public virtual bool IsEnabled => true;

        public virtual bool IsFontUnderlineEnabled => true;

        /// <summary>
        /// Use char or word wrapping for the text strings. False==word wrapping, true==char wrapping.
        /// </summary>
        public virtual bool IsUseCharWrapping => false;

        /// <summary>
        /// IETF language code (such as "en-US"). https://en.wikipedia.org/wiki/IETF_language_tag
        /// Use ISO 639 and ISO 3166-1 alpha-2 codes.
        /// Please note that Icon texture path is automatically generated based on this code.
        /// </summary>
        public abstract string LanguageTag { get; }

        public string[] LanguageTagWithFallbackLanguages
            => (this.FallbackLanguages ?? new string[0])
               .ConcatOne(this.LanguageTag)
               .ToArray();

        public abstract string NameEnglish { get; }

        public abstract string NameNative { get; }
    }
}