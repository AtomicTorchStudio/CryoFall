namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Explosives;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentBombCountdown : ClientComponent
    {
        private BombCountdownDisplay control;

        private bool isRendering = true;

        private int? lastDisplayedValue;

        private double secondsRemains;

        private TextBlock textBlock;

        public bool IsRendering
        {
            get => this.isRendering;
            set
            {
                if (this.isRendering == value)
                {
                    return;
                }

                this.isRendering = value;
                this.RefreshRendering();
            }
        }

        public double SecondsRemains => this.secondsRemains;

        public void Setup(double secondsRemains, Vector2D positionOffset)
        {
            this.secondsRemains = secondsRemains;

            this.control = new BombCountdownDisplay();
            Client.UI.AttachControl(
                this.SceneObject,
                this.control,
                positionOffset,
                isFocusable: false);
            this.textBlock = this.control.TextBlock;
            this.RefreshRendering();

            this.Update(0);
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            this.secondsRemains -= deltaTime;

            var truncatedValue = (int)(this.secondsRemains + 1);
            truncatedValue = Math.Max(0, truncatedValue);

            if (this.lastDisplayedValue == truncatedValue)
            {
                return;
            }

            this.lastDisplayedValue = truncatedValue;
            this.textBlock.Text = truncatedValue.ToString();
        }

        private void RefreshRendering()
        {
            if (this.control != null)
            {
                this.control.Visibility = this.isRendering
                                              ? Visibility.Visible
                                              : Visibility.Collapsed;
            }
        }
    }
}