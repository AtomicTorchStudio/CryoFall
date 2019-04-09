namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data
{
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentWorldMapCurrentCharacterUpdater : ClientComponent
    {
        private FrameworkElement currentCharacterControl;

        private WorldMapController worldMapController;

        private Vector2D worldPosition;

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

                Canvas.SetLeft(this.currentCharacterControl, canvasPosition.X);
                Canvas.SetTop(this.currentCharacterControl, canvasPosition.Y);
            }
        }

        public void Setup(WorldMapController worldMapController, FrameworkElement currentCharacterControl)
        {
            this.currentCharacterControl = currentCharacterControl;
            this.worldMapController = worldMapController;
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);

            var character = Client.Characters.CurrentPlayerCharacter;
            this.WorldPosition = character?.Position ?? Vector2D.Zero;
        }
    }
}