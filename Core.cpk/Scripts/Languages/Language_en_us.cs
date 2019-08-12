namespace AtomicTorch.CBND.CoreMod.Languages
{
    using AtomicTorch.CBND.CoreMod.ClientLanguages;

    public class Language_en_us : ProtoLanguageDefinition
    {
        public override string AcceptText => "Accept";

        public override string LanguageTag => "en_us";

        public override string NameEnglish => "English";

        public override string NameNative => this.NameEnglish;
    }
}