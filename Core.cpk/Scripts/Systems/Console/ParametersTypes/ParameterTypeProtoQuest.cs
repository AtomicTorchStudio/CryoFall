namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Quests;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeProtoQuest : BaseConsoleCommandParameterProtoType<IProtoQuest>
    {
        public override string ShortDescription => "quest name";
    }
}