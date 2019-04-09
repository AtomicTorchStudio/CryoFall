namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeProtoStructure : BaseConsoleCommandParameterProtoType<IProtoObjectStructure>
    {
        public override string ShortDescription => "proto structure name";
    }
}