namespace AtomicTorch.CBND.CoreMod.Languages
{
    using AtomicTorch.CBND.GameApi.Data;

    public class Language_zh_cn : ProtoLanguageDefinition
    {
        public override string AcceptText => "接受";

        public override bool IsEnabled => false; // the translation is coming soon!

        public override string LanguageTag => "zh_cn";

        public override string NameEnglish => "Chinese Simplified";

        public override string NameNative => "简体中文";
    }
}