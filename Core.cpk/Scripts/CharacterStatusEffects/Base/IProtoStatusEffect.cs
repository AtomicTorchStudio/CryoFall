namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects
{
    using System.Collections.Generic;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IProtoStatusEffect : IProtoLogicObject
    {
        string Description { get; }

        StatusEffectDisplayMode DisplayMode { get; }

        ITextureResource Icon { get; }

        double IntensityAutoDecreasePerSecondValue { get; }

        bool IsPublic { get; }

        bool IsRemovedOnRespawn { get; }

        StatusEffectKind Kind { get; }

        IReadOnlyStatsDictionary ProtoEffects { get; }

        double VisibilityIntensityThreshold { get; }

        void ClientTooltipCreateControls(List<UIElement> controls);

        ITextureResource GetColorizedIcon(double effectIntensity);

        void ServerAddIntensity(ILogicObject statusEffect, double intensityToAdd);

        void ServerSetup(ILogicObject statusEffect, ICharacter character);
    }
}