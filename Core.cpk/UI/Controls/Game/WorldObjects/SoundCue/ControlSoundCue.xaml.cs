namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.SoundCue
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media.Animation;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    public partial class ControlSoundCue : BaseUserControl, ICacheableControl
    {
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register(nameof(Angle),
                                        typeof(double),
                                        typeof(ControlSoundCue),
                                        new PropertyMetadata(default(double)));

        private Panel layoutRoot;

        private Storyboard storyboard;

        public double Angle
        {
            get => (double)this.GetValue(AngleProperty);
            set => this.SetValue(AngleProperty, value);
        }

        public void ResetControlForCache()
        {
        }

        public void ShowAt(Vector2D soundWorldPosition, in BoundsDouble viewBounds)
        {
            this.Angle = this.CalculateAngle(soundWorldPosition, viewBounds);

            soundWorldPosition = viewBounds.ClampInside(soundWorldPosition);

            var screenPosition = Api.Client.Input.WorldToScreenPosition(soundWorldPosition);
            var scale = 1 / Api.Client.UI.GetScreenScaleCoefficient();
            // adjust with the reverse screen scale coefficient
            screenPosition *= scale;

            Api.Client.UI.LayoutRootChildren.Add(this);

            Canvas.SetLeft(this.layoutRoot, screenPosition.X);
            Canvas.SetTop(this.layoutRoot, screenPosition.Y);
        }

        protected override void InitControl()
        {
            this.layoutRoot = this.GetByName<Grid>("LayoutRoot");
            this.storyboard = this.GetResource<Storyboard>("Storyboard");
        }

        protected override void OnLoaded()
        {
            this.storyboard.Begin(this.layoutRoot);

            ClientTimersSystem.AddAction(
                delaySeconds: 1,
                () =>
                {
                    ((Panel)this.Parent).Children.Remove(this);
                    ControlsCache<ControlSoundCue>.Instance.Push(this);
                });
        }

        private double CalculateAngle(in Vector2D soundWorldPosition, in BoundsDouble viewBounds)
        {
            var delta = (viewBounds.Offset + viewBounds.Size / 2) - soundWorldPosition;
            return -90 + 180 * Math.Atan2(delta.X, delta.Y) / Math.PI;
        }
    }
}