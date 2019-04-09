namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelStatusEffect : BaseViewModel
    {
        private const byte BackgroundOpacity = 0xAA;

        private static readonly Brush BrushBuffTier0
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0x33, 0x88, 0x33));

        private static readonly Brush BrushBuffTier1
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0x22, 0xAA, 0x22));

        private static readonly Brush BrushBuffTier2
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0x00, 0xCC, 0x00));

        private static readonly Brush BrushDebuffTier0
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0xFF, 0xAA, 0x22));

        private static readonly Brush BrushDebuffTier1
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0xFF, 0x77, 0x22));

        private static readonly Brush BrushDebuffTier2
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0xFF, 0x22, 0x22));

        private static readonly Brush BrushNeutral
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0xCC, 0xCC, 0xCC));

        private readonly IProtoStatusEffect protoStatusEffect;

        private readonly StatusEffectPublicState publicState;

        private readonly ILogicObject statusEffect;

        public ViewModelStatusEffect(ILogicObject statusEffect)
        {
            this.statusEffect = statusEffect;
            this.protoStatusEffect = (IProtoStatusEffect)statusEffect.ProtoLogicObject;

            this.publicState = statusEffect.GetPublicState<StatusEffectPublicState>();
            this.publicState.ClientSubscribe(
                _ => _.Intensity,
                _ => this.UpdateIntensity(),
                this);

            this.UpdateIntensity();
        }

        public Brush BackgroundBrush { get; private set; }

        public string Description => this.protoStatusEffect.Description;

        public Brush Icon
        {
            get
            {
                var icon = this.protoStatusEffect.ClientGetIcon(this.statusEffect);
                return Api.Client.UI.GetTextureBrush(icon);
            }
        }

        public byte IntensityPercent { get; private set; }

        public bool IsFlickering { get; private set; }

        public IProtoStatusEffect ProtoStatusEffect => this.protoStatusEffect;

        //public string IntensityPercentText { get; private set; }

        public string Title => this.protoStatusEffect.Name;

        public Visibility Visibility { get; private set; } = Visibility.Collapsed;

        public Visibility VisibilityIntensityPercent
            => this.protoStatusEffect.IsIntensityPercentVisible
                   ? Visibility.Visible
                   : Visibility.Collapsed;

        public static Brush GetBrush(StatusEffectKind kind, double intensity)
        {
            byte tier;
            if (intensity < 0.333)
            {
                tier = 0;
            }
            else if (intensity < 0.667)
            {
                tier = 1;
            }
            else
            {
                tier = 2;
            }

            switch (kind)
            {
                case StatusEffectKind.Buff:
                    switch (tier)
                    {
                        case 0:
                            return BrushBuffTier0;
                        case 1:
                            return BrushBuffTier1;
                        case 2:
                            return BrushBuffTier2;
                    }

                    break;

                case StatusEffectKind.Debuff:
                    switch (tier)
                    {
                        case 0:
                            return BrushDebuffTier0;
                        case 1:
                            return BrushDebuffTier1;
                        case 2:
                            return BrushDebuffTier2;
                    }

                    break;

                case StatusEffectKind.Neutral:
                    return BrushNeutral;
            }

            throw new Exception("Impossible");
        }

        public void Flicker()
        {
            this.IsFlickering = true;
            this.IsFlickering = false;
        }

        private void UpdateIntensity()
        {
            var intensity = this.publicState.Intensity;
            var percent = (byte)Math.Round(intensity * 100, MidpointRounding.AwayFromZero);
            this.IntensityPercent = percent;
            this.Visibility = intensity >= this.protoStatusEffect.VisibilityIntensityThreshold
                                  ? Visibility.Visible
                                  : Visibility.Collapsed;

            this.BackgroundBrush = GetBrush(this.protoStatusEffect.Kind, intensity);
        }
    }
}