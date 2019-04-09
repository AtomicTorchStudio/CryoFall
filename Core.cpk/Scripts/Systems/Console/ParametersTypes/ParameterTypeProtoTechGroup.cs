namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Technologies;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeProtoTechGroup : BaseConsoleCommandParameterProtoType<TechGroup>
    {
        public override string ShortDescription => "tech group name";
    }
}