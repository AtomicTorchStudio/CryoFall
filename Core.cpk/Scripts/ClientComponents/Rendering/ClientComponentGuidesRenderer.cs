namespace AtomicTorch.CBND.CoreMod.ClientComponents.Rendering
{
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public class ClientComponentGuidesRenderer : ClientComponent
    {
        private static readonly IInputClientService Input = Client.Input;

        private static ClientComponentGuidesRenderer instance;

        private Line[] lines;

        public ClientComponentGuidesRenderer() : base(isLateUpdateEnabled: true)
        {
        }

        public static void Refresh()
        {
            var isDrawing = ClientDebugGuidesManager.Instance.IsGuidesEnabled;
            if (!isDrawing)
            {
                // instance is not needed
                if (instance == null)
                {
                    return;
                }

                instance.SceneObject.Destroy();
                instance = null;
                return;
            }

            if (instance == null)
            {
                instance = Client.Scene.CreateSceneObject("Guide lines renderer")
                                 .AddComponent<ClientComponentGuidesRenderer>();
            }
        }

        public override void LateUpdate(double deltaTime)
        {
            var mouseWorldPosition = Input.MouseWorldPosition;
            var worldCellX = (int)mouseWorldPosition.X;
            var worldCellY = (int)mouseWorldPosition.Y;
            var screenPositionCurrent = Input.WorldToScreenPosition((worldCellX, worldCellY));
            var screenPositionNext = Input.WorldToScreenPosition((worldCellX + 1, worldCellY + 1));
            var screenWidth = Client.UI.ScreenWidth;
            var screenHeight = Client.UI.ScreenHeight;

            // adjust with the reverse screen scale coefficient
            var scale = 1 / Api.Client.UI.GetScreenScaleCoefficient();
            screenPositionCurrent *= scale;
            screenPositionNext *= scale;
            screenWidth *= scale;
            screenHeight *= scale;

            for (var index = 0; index < 4; index++)
            {
                var line = this.lines[index];
                switch (index)
                {
                    case 0:
                        // start vertical line
                        line.X1 = screenPositionCurrent.X;
                        line.X2 = screenPositionCurrent.X;
                        line.Y1 = 0;
                        line.Y2 = screenHeight;
                        break;

                    case 1:
                        // next vertical line
                        line.X1 = screenPositionNext.X;
                        line.X2 = screenPositionNext.X;
                        line.Y1 = 0;
                        line.Y2 = screenHeight;
                        break;

                    case 2:
                        // start horizontal line
                        line.X1 = 0;
                        line.X2 = screenWidth;
                        line.Y1 = screenPositionCurrent.Y;
                        line.Y2 = screenPositionCurrent.Y;
                        break;

                    case 3:
                        // next horizontal line
                        line.X1 = 0;
                        line.X2 = screenWidth;
                        line.Y1 = screenPositionNext.Y;
                        line.Y2 = screenPositionNext.Y;
                        break;
                }
            }
        }

        protected override void OnDisable()
        {
            foreach (var line in this.lines)
            {
                Api.Client.UI.LayoutRootChildren.Remove(line);
            }

            this.lines = null;
        }

        protected override void OnEnable()
        {
            var brush = new SolidColorBrush(Color.FromArgb(0x77, 0xFF, 0xFF, 0xFF));
            this.lines = new Line[4];
            for (var i = 0; i < 4; i++)
            {
                var line = new Line
                {
                    StrokeThickness = 1.5f,
                    Stroke = brush
                };

                Api.Client.UI.LayoutRootChildren.Add(line);
                this.lines[i] = line;
            }
        }
    }
}