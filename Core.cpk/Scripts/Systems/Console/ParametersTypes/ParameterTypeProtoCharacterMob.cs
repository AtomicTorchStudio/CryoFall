namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Characters;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeProtoCharacterMob : BaseConsoleCommandParameterProtoType<IProtoCharacterMob>
    {
        public override string ShortDescription => "proto mob name";
    }
}