namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Technologies;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeProtoTechNode : BaseConsoleCommandParameterProtoType<TechNode>
    {
        public override string ShortDescription => "tech node name";
    }
}