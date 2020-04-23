namespace AtomicTorch.CBND.CoreMod.Systems.Console
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Achievements;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ParameterTypeProtoAchievement : BaseConsoleCommandParameterProtoType<IProtoAchievement>
    {
        public override string ShortDescription => "achievement name";
    }
}