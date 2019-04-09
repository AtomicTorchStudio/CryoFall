namespace AtomicTorch.CBND.CoreMod.ClientOptions
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Player;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Helpers;

    public abstract class ProtoOptionSlider<TProtoOptionsCategory>
        : ProtoOption<TProtoOptionsCategory, double>
        where TProtoOptionsCategory : ProtoOptionsCategory, new()
    {
        public abstract double Maximum { get; }

        public abstract double Minimum { get; }

        public virtual double StepSize => (this.Maximum - this.Minimum) / 10;

        public override void RegisterValueType(IClientStorage storage)
        {
            // System.Double is already registered
        }

        public override double RoundValue(double value)
        {
            return Math.Round(value, 2, MidpointRounding.AwayFromZero);
        }

        protected override double ClampValue(double value)
        {
            return MathHelper.Clamp(value, this.Minimum, this.Maximum);
        }

        protected override void CreateControlInternal(
            out FrameworkElement labelControl,
            out FrameworkElement optionControl)
        {
            labelControl = new FormattedTextBlock()
            {
                Content = this.Name,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 7, 0, 0)
            };

            var smallChange = this.StepSize;
            var slider = new Slider
            {
                Minimum = this.Minimum,
                Maximum = this.Maximum,
                SmallChange = smallChange,
                LargeChange = smallChange,
                VerticalAlignment = VerticalAlignment.Center,
                Width = 200,
                Margin = new Thickness(-10, 2, 0, -3),
                IsSnapToTickEnabled = true,
                TickFrequency = smallChange
            };

            this.SetupOptionToControlValueBinding(slider, RangeBase.ValueProperty);

            this.SetupSliderControlTooltip(slider);

            optionControl = slider;
        }

        protected virtual void SetupSliderControlTooltip(Slider slider)
        {
            var textBlock = new TextBlock();
            var tooltipBinding = new Binding("Value");
            this.SetupTooltipBinding(tooltipBinding);
            textBlock.SetBinding(
                TextBlock.TextProperty,
                tooltipBinding);

            textBlock.DataContext = slider;
            ToolTipServiceExtend.SetToolTip(slider, textBlock);
        }

        protected virtual void SetupTooltipBinding(Binding tooltipBinding)
        {
            // by default represent value as percents
            tooltipBinding.Converter = new ValueToPercentsConverter();
        }
    }
}