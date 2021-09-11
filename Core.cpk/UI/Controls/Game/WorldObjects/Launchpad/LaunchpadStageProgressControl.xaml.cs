namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Launchpad
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Shapes;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class LaunchpadStageProgressControl : BaseUserControl
    {
        public static readonly DependencyProperty CurrentStageIndexProperty
            = DependencyProperty.Register("CurrentStageIndex",
                                          typeof(byte),
                                          typeof(LaunchpadStageProgressControl),
                                          new PropertyMetadata(default(byte)));

        public static readonly DependencyProperty MaxStageIndexProperty =
            DependencyProperty.Register(nameof(MaxStageIndex),
                                        typeof(byte),
                                        typeof(LaunchpadStageProgressControl),
                                        new PropertyMetadata(default(byte)));

        public byte CurrentStageIndex
        {
            get => (byte)this.GetValue(CurrentStageIndexProperty);
            set => this.SetValue(CurrentStageIndexProperty, value);
        }

        public byte MaxStageIndex
        {
            get => (byte)this.GetValue(MaxStageIndexProperty);
            set => this.SetValue(MaxStageIndexProperty, value);
        }

        protected override void OnLoaded()
        {
            var layoutRoot = this.GetByName<Canvas>("LayoutRoot");

            var currentStageIndex = this.CurrentStageIndex;
            var maxStageIndex = this.MaxStageIndex;

            var styleCompletedStageEllipse = this.GetResource<Style>("CompletedStageEllipseStyle");
            var styleNonCompletedStageEllipse = this.GetResource<Style>("NonCompletedStageEllipseStyle");

            var styleCompletedStageBackgroundRectangle =
                this.GetResource<Style>("CompletedStageBackgroundRectangleStyle");
            var styleNonCompletedStageBackgroundRectangle =
                this.GetResource<Style>("NonCompletedStageBackgroundRectangleStyle");
            var styleLastCompletedStageBackgroundRectangle =
                this.GetResource<Style>("LastCompletedStageBackgroundRectangleStyle");

            var styleCompletedStageTextBlock = this.GetResource<Style>("CompletedStageTextBlockStyle");
            var styleNonCompletedStageTextBlock = this.GetResource<Style>("NonCompletedStageTextBlockStyle");

            var ellipseSize = this.GetResource<double>("EllipseSize");
            var rectangleHeight = this.GetResource<double>("RectangleHeight");
            var availableWidth = this.ActualWidth;
            var stagePaddingLeft = availableWidth / maxStageIndex;
            // calculate and apply canvas left offset (to ensure perfect centering)
            layoutRoot.Margin = new Thickness(stagePaddingLeft / 2 - ellipseSize / 2, 0, 0, 0);

            for (var stageIndex = 1; stageIndex <= maxStageIndex; stageIndex++)
            {
                var container = new Grid()
                {
                    Width = ellipseSize,
                    Height = ellipseSize
                };

                Canvas.SetLeft(container, stagePaddingLeft * (stageIndex - 1));

                if (stageIndex != maxStageIndex)
                {
                    // add progress rectangle (behind the ellipse)
                    var rectangle = new Rectangle()
                    {
                        Width = stagePaddingLeft,
                        Style = stageIndex <= currentStageIndex
                                    ? stageIndex == currentStageIndex
                                      && stageIndex != maxStageIndex
                                          ? styleLastCompletedStageBackgroundRectangle
                                          : styleCompletedStageBackgroundRectangle
                                    : styleNonCompletedStageBackgroundRectangle
                    };

                    // ensure the rectangle is vertically centered
                    Canvas.SetTop(rectangle, -rectangleHeight / 2);

                    var rectangleCanvas = new Canvas
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    rectangleCanvas.Children.Add(rectangle);
                    container.Children.Add(rectangleCanvas);
                }

                var ellipse = new Ellipse()
                {
                    Style = stageIndex <= currentStageIndex
                                ? styleCompletedStageEllipse
                                : styleNonCompletedStageEllipse
                };

                container.Children.Add(ellipse);

                container.Children.Add(new TextBlock()
                {
                    Text = stageIndex.ToString(),
                    Style = stageIndex <= currentStageIndex
                                ? styleCompletedStageTextBlock
                                : styleNonCompletedStageTextBlock
                });

                layoutRoot.Children.Add(container);
            }
        }
    }
}