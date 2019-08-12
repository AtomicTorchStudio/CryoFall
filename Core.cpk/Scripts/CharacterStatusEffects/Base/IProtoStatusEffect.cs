namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects
{
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IProtoStatusEffect : IProtoLogicObject
    {
        string Description { get; }

        bool IsIntensityPercentVisible { get; }

        bool IsRemovedOnRespawn { get; }

        StatusEffectKind Kind { get; }

        IReadOnlyStatsDictionary ProtoEffects { get; }

        double VisibilityIntensityThreshold { get; }

        ITextureResource Icon { get; }

        void ServerAddIntensity(ILogicObject statusEffect, double intensityToAdd);

        void ServerSetup(ILogicObject statusEffect, ICharacter character);
    }
}