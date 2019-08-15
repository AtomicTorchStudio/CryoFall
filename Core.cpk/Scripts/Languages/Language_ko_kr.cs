namespace AtomicTorch.CBND.CoreMod.Languages
{
    using AtomicTorch.CBND.CoreMod.ClientLanguages;

    public class Language_ko_kr : ProtoLanguageDefinition
    {
        public override string AcceptText => "받아 들인다";

        public override bool IsFontUnderlineEnabled => false;

        public override bool IsUseCharWrapping => true;

        public override string LanguageTag => "ko_kr";

        public override string NameEnglish => "Korean";

        public override string NameNative => "한국어";
    }
}