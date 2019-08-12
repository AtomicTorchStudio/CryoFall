namespace AtomicTorch.CBND.CoreMod.Languages
{
    using AtomicTorch.CBND.CoreMod.ClientLanguages;

    public class Language_zh_cn : ProtoLanguageDefinition
    {
        public override string AcceptText => "接受";

        public override bool IsEnabled => true;

        public override bool IsFontUnderlineEnabled => false;

        public override string LanguageTag => "zh_cn";

        public override string NameEnglish => "Chinese Simplified";

        public override string NameNative => "简体中文";
    }
}