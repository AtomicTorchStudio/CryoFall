namespace AtomicTorch.CBND.CoreMod.Languages
{
    using AtomicTorch.CBND.CoreMod.ClientLanguages;

    public class Language_ja_jp : ProtoLanguageDefinition
    {
        public override string AcceptText => "つづく";

        public override bool IsFontUnderlineEnabled => false;

        public override bool IsUseCharWrapping => true;

        public override string LanguageTag => "ja_jp";

        public override string NameEnglish => "Japanese";

        public override string NameNative => "日本語";
    }
}