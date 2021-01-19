namespace AtomicTorch.CBND.CoreMod.Systems.Notifications
{
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class NotificationBrushes
    {
        private const byte BackgroundAlpha = 0x99;

        private const byte BorderAlpha = 0xFF;

        public static readonly Brush BrushBackgroundBad
            = new SolidColorBrush(
                Api.Client.UI.GetApplicationResource<Color>("ColorRed3")
                   .WithAlpha(BackgroundAlpha));

        public static readonly Brush BrushBorderBad
            = new SolidColorBrush(
                Api.Client.UI.GetApplicationResource<Color>("ColorRed0")
                   .WithAlpha(BorderAlpha));

        public static readonly Brush BrushBackgroundGood
            = new SolidColorBrush(
                Api.Client.UI.GetApplicationResource<Color>("ColorGreen3")
                   .WithAlpha(BackgroundAlpha));

        public static readonly Brush BrushBorderGood
            = new SolidColorBrush(
                Api.Client.UI.GetApplicationResource<Color>("ColorGreen0")
                   .WithAlpha(BorderAlpha));

        public static readonly Brush BrushBackgroundNeutral
            = new SolidColorBrush(
                Api.Client.UI.GetApplicationResource<Color>("ColorAlt2")
                   .WithAlpha(BackgroundAlpha));

        public static readonly Brush BrushBorderNeutral
            = new SolidColorBrush(
                Api.Client.UI.GetApplicationResource<Color>("ColorAlt0")
                   .WithAlpha(BorderAlpha));

        public static readonly Brush BrushBackgroundEvent
            = new SolidColorBrush(
                Api.Client.UI.GetApplicationResource<Color>("Color2")
                   .WithAlpha(BackgroundAlpha));

        public static readonly Brush BrushBorderEvent
            = new LinearGradientBrush()
            {
                GradientStops = new GradientStopCollection()
                {
                    new() { Color = Api.Client.UI.GetApplicationResource<Color>("Color6") },
                    new() { Color = Api.Client.UI.GetApplicationResource<Color>("Color5"), Offset = 1.0 }
                },
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0,   1),
                Opacity = BorderAlpha / (double)byte.MaxValue
            };
    }
}