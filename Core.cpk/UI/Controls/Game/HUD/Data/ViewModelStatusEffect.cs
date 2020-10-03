namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using JetBrains.Annotations;

    public class ViewModelStatusEffect : BaseViewModel
    {
        private const byte BackgroundOpacity = 0x88;

        public static readonly Brush BrushBuffTier0
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0x44, 0xAA, 0x44));

        public static readonly Brush BrushBuffTier1
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0x33, 0xBB, 0x33));

        public static readonly Brush BrushBuffTier2
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0x00, 0xCC, 0x00));

        public static readonly Brush BrushDebuffTier0
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0xFF, 0xCC, 0x33));

        public static readonly Brush BrushDebuffTier1
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0xFF, 0x88, 0x22));

        public static readonly Brush BrushDebuffTier2
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0xFF, 0x11, 0x11));

        public static readonly Brush BrushNeutral
            = new SolidColorBrush(Color.FromArgb(BackgroundOpacity, 0xBB, 0xBB, 0xBB));

        private readonly Lazy<IReadOnlyList<StatModificationData>> lazyEffects;

        private readonly IProtoStatusEffect protoStatusEffect;

        private readonly StatusEffectPublicState publicState;

        private readonly ILogicObject statusEffect;

        private bool isFlickerScheduled;

        public ViewModelStatusEffect(ILogicObject statusEffect)
        {
            this.statusEffect = statusEffect;
            this.protoStatusEffect = (IProtoStatusEffect)statusEffect.ProtoLogicObject;
            this.lazyEffects = new Lazy<IReadOnlyList<StatModificationData>>(this.CreateEffectsList);

            this.publicState = statusEffect.GetPublicState<StatusEffectPublicState>();
            this.publicState.ClientSubscribe(
                _ => _.Intensity,
                _ => this.UpdateIntensity(),
                this);

            this.UpdateIntensity();

            this.IsFlickerScheduled = true;
        }

        public Brush BackgroundBrush { get; private set; }

        public string Description => this.protoStatusEffect.Description;

        [CanBeNull]
        public IReadOnlyList<StatModificationData> Effects => this.lazyEffects.Value;

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.protoStatusEffect.Icon);

        public byte IntensityPercent { get; private set; }

        public bool IsFlickerScheduled
        {
            get
            {
                var result = this.isFlickerScheduled;
                this.IsFlickerScheduled = false;
                return result;
            }
            private set => this.SetProperty(ref this.isFlickerScheduled, value);
        }

        public bool IsIconIntensityPercentVisible
            => this.protoStatusEffect.DisplayMode.HasFlag(StatusEffectDisplayMode.IconShowIntensityPercent);

        public bool IsIconPlaceholderVisible
            => !this.protoStatusEffect.DisplayMode.HasFlag(StatusEffectDisplayMode.IconShowIntensityPercent)
               && !this.protoStatusEffect.DisplayMode.HasFlag(StatusEffectDisplayMode.IconShowTimeRemains);

        public bool IsIconTimeRemainsVisible
            => this.protoStatusEffect.DisplayMode.HasFlag(StatusEffectDisplayMode.IconShowTimeRemains);

        public bool IsTooltipIntensityPercentVisible
            => this.protoStatusEffect.DisplayMode.HasFlag(StatusEffectDisplayMode.TooltipShowIntensityPercent);

        public bool IsTooltipTimeRemainsVisible
            => this.protoStatusEffect.DisplayMode.HasFlag(StatusEffectDisplayMode.TooltipShowTimeRemains);

        public IProtoStatusEffect ProtoStatusEffect => this.protoStatusEffect;

        public string TimeRemains { get; private set; }

        public string Title => this.protoStatusEffect.Name;

        public Visibility Visibility { get; private set; } = Visibility.Collapsed;

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
            this.IsFlickerScheduled = true;
        }

        private IReadOnlyList<StatModificationData> CreateEffectsList()
        {
            var effects = this.protoStatusEffect.ProtoEffects;
            var multipliers = effects.Multipliers;
            var totalEntriesCount = multipliers.Count + effects.Values.Count;

            if (totalEntriesCount == 0)
            {
                return null;
            }

            var result = new Dictionary<StatName, StatModificationData>(capacity: totalEntriesCount);

            foreach (var entry in effects.Values)
            {
                AppendValue(entry.Key, entry.Value);
            }

            foreach (var entry in multipliers)
            {
                AppendPercent(entry.Key, entry.Value);
            }

            return result.Values.OrderBy(p => p.StatName)
                         .ToList();

            void AppendValue(StatName key, double value)
            {
                if (result.TryGetValue(key, out var entry))
                {
                    entry.Value += value;
                    return;
                }

                result[key] = new StatModificationData(key,
                                                       value,
                                                       percent: 1.0);
            }

            void AppendPercent(StatName key, double percent)
            {
                if (result.TryGetValue(key, out var entry))
                {
                    entry.Percent += percent;
                    return;
                }

                result[key] = new StatModificationData(key,
                                                       value: 0,
                                                       percent);
            }
        }

        private void UpdateIntensity()
        {
            var intensity = this.publicState.Intensity;

            if (this.protoStatusEffect.IntensityAutoDecreasePerSecondValue > 0)
            {
                var secondsRemains = intensity / this.protoStatusEffect.IntensityAutoDecreasePerSecondValue;
                this.TimeRemains = secondsRemains > 60
                                       ? ClientTimeFormatHelper.FormatTimeDuration(secondsRemains)
                                       : Math.Ceiling(secondsRemains).ToString("F0")
                                         + ClientTimeFormatHelper.SuffixSeconds;
            }

            this.IntensityPercent = (byte)Math.Ceiling(intensity * 100);

            var wasVisible = this.Visibility == Visibility.Visible;
            var isVisible = intensity >= this.protoStatusEffect.VisibilityIntensityThreshold;
            this.Visibility = isVisible
                                  ? Visibility.Visible
                                  : Visibility.Collapsed;

            if (!wasVisible && isVisible)
            {
                this.IsFlickerScheduled = true;
            }

            this.BackgroundBrush = GetBrush(this.protoStatusEffect.Kind, intensity);
        }
    }
}