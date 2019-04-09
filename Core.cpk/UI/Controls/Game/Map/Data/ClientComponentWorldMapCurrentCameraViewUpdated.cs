namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentWorldMapCurrentCameraViewUpdated : ClientComponent
    {
        private Vector2D cameraViewWorldSize;

        private FrameworkElement control;

        private int scale;

        private WorldMapController worldMapController;

        private Vector2D worldPosition;

        public Vector2D CameraViewWorldSize
        {
            get => this.cameraViewWorldSize;
            set
            {
                if (this.cameraViewWorldSize == value)
                {
                    return;
                }

                this.cameraViewWorldSize = value;
                this.control.Width = value.X * this.scale;
                this.control.Height = value.Y * this.scale;
            }
        }

        public Vector2D CanvasPosition { get; private set; }

        public Vector2D WorldPosition
        {
            get => this.worldPosition;
            private set
            {
                if (this.worldPosition == value)
                {
                    return;
                }

                this.worldPosition = value;
                var canvasPosition = this.worldMapController.WorldToCanvasPosition(this.worldPosition);
                this.CanvasPosition = canvasPosition;

                Canvas.SetLeft(this.control, canvasPosition.X);
                Canvas.SetTop(this.control, canvasPosition.Y - this.control.Height);
            }
        }

        public void Setup(WorldMapController worldMapController, FrameworkElement control, int scale)
        {
            this.control = control;
            this.worldMapController = worldMapController;
            this.scale = scale;
        }

        public override void Update(double deltaTime)
        {
            var bounds = Client.Rendering.WorldCameraCurrentViewWorldBounds;
            this.CameraViewWorldSize = bounds.Size;
            this.WorldPosition = bounds.Offset;
        }
    }
}