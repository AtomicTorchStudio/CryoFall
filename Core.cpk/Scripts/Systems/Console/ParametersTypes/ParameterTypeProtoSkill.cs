namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Skills;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeProtoSkill : BaseConsoleCommandParameterProtoType<IProtoSkill>
    {
        public override string ShortDescription => "skill name";
    }
}