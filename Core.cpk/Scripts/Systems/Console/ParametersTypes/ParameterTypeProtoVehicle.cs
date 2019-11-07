namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Vehicles;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeProtoVehicle : BaseConsoleCommandParameterProtoType<IProtoVehicle>
    {
        public override string ShortDescription => "proto vehicle name";
    }
}