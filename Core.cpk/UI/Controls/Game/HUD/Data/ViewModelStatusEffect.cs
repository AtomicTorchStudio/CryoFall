namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ViewModelStatusEffect : BaseViewModel
    {
        private readonly IProtoStatusEffect protoStatusEffect;

        private readonly StatusEffectPublicState publicState;

        private bool isFlickerScheduled;

        private Brush lastBrush;

        private double lastIntensity;

        public ViewModelStatusEffect(ILogicObject statusEffect)
        {
            this.protoStatusEffect = (IProtoStatusEffect)statusEffect.ProtoLogicObject;
            this.StatsDictionary = this.protoStatusEffect.ProtoEffects;

            this.publicState = statusEffect.GetPublicState<StatusEffectPublicState>();
            this.publicState.ClientSubscribe(
                _ => _.Intensity,
                _ => this.UpdateIntensity(),
                this);

            this.UpdateIntensity();

            this.IsFlickerScheduled = true;

            var controls = new List<UIElement>();
            this.PopulateControls(controls);

            if (controls.Count > 0)
            {
                this.InfoControls = controls;
            }
        }

        public Brush ColorizedIcon
            => Api.Client.UI.GetTextureBrush(
                this.protoStatusEffect.GetColorizedIcon(this.lastIntensity));

        public string Description => this.protoStatusEffect.Description;

        public IReadOnlyList<UIElement> InfoControls { get; }

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

        public string Name => this.protoStatusEffect.Name;

        public IProtoStatusEffect ProtoStatusEffect => this.protoStatusEffect;

        public IReadOnlyStatsDictionary StatsDictionary { get; }

        public string TimeRemains { get; private set; }

        public string Title => this.protoStatusEffect.Name;

        public Visibility Visibility { get; private set; } = Visibility.Collapsed;

        public void Flicker()
        {
            this.IsFlickerScheduled = true;
        }

        private void PopulateControls(List<UIElement> controls)
        {
            this.protoStatusEffect.ClientTooltipCreateControls(controls);
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

            this.lastIntensity = intensity;
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

            var brush = ClientStatusEffectIconColorizer.GetBrush(this.protoStatusEffect.Kind, intensity);
            if (this.lastBrush == brush)
            {
                return;
            }

            this.lastBrush = brush;
            this.NotifyPropertyChanged(nameof(this.ColorizedIcon));
        }
    }
}