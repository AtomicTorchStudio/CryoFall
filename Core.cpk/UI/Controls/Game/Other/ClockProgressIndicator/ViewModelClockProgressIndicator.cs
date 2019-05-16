namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Other.ClockProgressIndicator
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ViewModelClockProgressIndicator : BaseViewModel
    {
        private readonly bool isReversed;

        private readonly StreamGeometry streamGeometry
#if !GAME
            // placeholder geometry for design-time
            = (StreamGeometry)Geometry.Parse(
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                "M27.499605,-20.5 L27.749607,26 L68.249993,-10 L68.999993,44.75 L25.749588,61 L-14.050003,41.921318 L-13.549998,2.4213181 z");

#else
		= new StreamGeometry();
#endif

        public ViewModelClockProgressIndicator(
            bool isReversed,
            bool isAutoDisposeFields = true) : base(
            isAutoDisposeFields)
        {
            this.isReversed = isReversed;
        }

        public StreamGeometry StreamGeometry => this.streamGeometry;

        private double? lastProgressFraction;

        private double progressFraction;

        private Vector2Ushort controlSize;

        public double ProgressFraction
        {
            get => this.progressFraction;
            set
            {
                value = MathHelper.Clamp(value, 0, 1);
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (this.progressFraction == value)
                {
                    return;
                }

                this.progressFraction = value;
                // do not bind to this property!
                //this.NotifyThisPropertyChanged();

                if (this.lastProgressFraction.HasValue
                    && Math.Abs(this.lastProgressFraction.Value - this.progressFraction) < 0.001)
                {
                    // the change is too small to consider rebuilding geometry
                    return;
                }

                this.ForceRefreshGeometry();
            }
        }

        private void ForceRefreshGeometry()
        {
            this.lastProgressFraction = this.progressFraction;
            var fraction = this.progressFraction;

            CreateClockCutoutGeometry(fraction,
                                      this.isReversed,
                                      this.controlSize.X,
                                      this.controlSize.Y,
                                      this.streamGeometry);
        }

        public Vector2Ushort ControlSize
        {
            get => this.controlSize;
            set
            {
                if (this.controlSize == value)
                {
                    return;
                }

                this.controlSize = value;
                this.ForceRefreshGeometry();
            }
        }

        private static void CreateClockCutoutGeometry(
            double value,
            bool isReversed,
            double width,
            double height,
            StreamGeometry streamGeometry)
        {
            //Api.Logger.WriteDev("Progress fraction: " + (value * 100).ToString("F2") + "%");

            value = MathHelper.Clamp(value, 0, 1);
            if (!isReversed) // that's right :-)
            {
                value = 1 - value;
            }

            double angleRad, angleDeg;
            {
                var angle = value * MathConstants.DoublePI;
                angleRad = angle - MathConstants.PI / 2;
                angleDeg = angle * MathConstants.RadToDeg;
            }

            Vector2D point = (width / 2 + width * 2 * Math.Cos(angleRad),
                              height / 2 + height * 2 * Math.Sin(angleRad));

            // let's build geometry to make a "clock"-style cutout
            using (var ctx = streamGeometry.Open())
            {
                ctx.BeginFigure(new Point(width / 2, 0), true, true);
                ctx.LineTo(new Point(width / 2,      height / 2), true, true);
                ctx.LineTo(new Point(point.X,        point.Y),    true, true);

                if (angleDeg < 45)
                {
                    ctx.LineTo(new Point(width, 0), true, true);
                }

                if (angleDeg < 180)
                {
                    ctx.LineTo(new Point(width, height), true, true);
                }

                if (angleDeg < 270)
                {
                    ctx.LineTo(new Point(0, height), true, true);
                }

                if (angleDeg < 315)
                {
                    ctx.LineTo(new Point(0, 0), true, true);
                }
            }
        }

        protected void ResetLastProgressFraction()
        {
            this.lastProgressFraction = null;
        }
    }
}