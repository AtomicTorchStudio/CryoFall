namespace AtomicTorch.CBND.CoreMod.Languages
{
    using AtomicTorch.CBND.GameApi.Data;

    public class Language_zh_tw : ProtoLanguageDefinition
    {
        public override string AcceptText => "接受";

        public override bool IsEnabled => false; // the translation is coming soon!

        public override string LanguageTag => "zh_tw";

        public override string NameEnglish => "Chinese Traditional";

        public override string NameNative => "繁體中文";
    }
}