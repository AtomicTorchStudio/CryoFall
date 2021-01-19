namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Events;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeProtoEvent : BaseConsoleCommandParameterProtoType<IProtoEvent>
    {
        public override string ShortDescription => "event name";
    }
}