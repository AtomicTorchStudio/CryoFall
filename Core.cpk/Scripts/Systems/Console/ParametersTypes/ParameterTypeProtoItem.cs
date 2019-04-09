namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.GameApi.Data.Items;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeProtoItem : BaseConsoleCommandParameterProtoType<IProtoItem>
    {
        public override string ShortDescription => "proto item name";
    }
}