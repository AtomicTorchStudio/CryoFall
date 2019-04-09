namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeProtoStatusEffect : BaseConsoleCommandParameterProtoType<IProtoStatusEffect>
    {
        public override string ShortDescription => "proto status effect name";
    }
}