namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.GameApi.Data.Zones;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeProtoZone : BaseConsoleCommandParameterProtoType<IProtoZone>
    {
        public override string ShortDescription => "zone name";
    }
}