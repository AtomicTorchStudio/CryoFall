namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class BaseClientComponentEditorToolSelectLocation : ClientComponent
    {
        protected static readonly IWorldClientService WorldService = Client.World;

        private Vector2D lastWorldPosition;

        private IClientSceneObject sceneObjectSelectionRectangle;

        private BoundsUshort selectionBounds;

        private Border selectionRectange;

        private Vector2Ushort selectionRectEndPosition;

        private Vector2Ushort selectionRectStartPosition;

        public event Action SelectionBoundsChanged;

        public BoundsUshort SelectionBounds => this.selectionBounds;

        protected Vector2Ushort CurrentMouseTilePosition { get; private set; }

        protected Vector2Ushort SelectionRectEndPosition
        {
            get => this.selectionRectEndPosition;
            set
            {
                this.selectionRectEndPosition = value;
                this.UpdateSelectionBounds();
            }
        }

        protected Vector2Ushort SelectionRectStartPosition
        {
            get => this.selectionRectStartPosition;
            set
            {
                this.selectionRectStartPosition = value;
                this.UpdateSelectionBounds();
            }
        }

        public void SetSelectionBounds(BoundsUshort newBounds)
        {
            this.selectionBounds = this.ClampToWorldBounds(newBounds);
            this.selectionRectStartPosition = newBounds.Offset;
            this.selectionRectEndPosition = (newBounds.Offset
                                             + newBounds.Size
                                             - Vector2Ushort.One).ToVector2Ushort();
            // do not invoke this otherwise ViewModels will not allow inputting values
            //this.SelectionBoundsChanged?.Invoke();
            this.UpdateSelectionRectange();
        }

        public override void Update(double deltaTime)
        {
            var worldPosition = ClientInputManager.MouseWorldPosition;
            this.CurrentMouseTilePosition = (Vector2Ushort)worldPosition;

            var isMoved = worldPosition != this.lastWorldPosition;
            this.lastWorldPosition = worldPosition;

            this.SceneObject.Position = this.CurrentMouseTilePosition.ToVector2D();

            if (ClientInputManager.IsButtonUp(GameButton.ActionUseCurrentItem))
            {
                this.OnSelectionEnded();
                return;
            }

            if (ClientInputManager.IsButtonDown(GameButton.ActionUseCurrentItem))
            {
                Client.UI.BlurFocus();
                this.OnSelectionStarted();
                return;
            }

            if (isMoved && ClientInputManager.IsButtonHeld(GameButton.ActionUseCurrentItem))
            {
                Client.UI.BlurFocus();
                this.OnSelectionExtended();
                this.UpdateSelectionRectange();
            }

            this.OnUpdated();
        }

        protected BoundsUshort ClampToWorldBounds(BoundsUshort bounds)
        {
            return WorldService.WorldBounds.ClampInside(bounds);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            this.selectionRectange = null;
            this.sceneObjectSelectionRectangle.Destroy();
            this.sceneObjectSelectionRectangle = null;
        }

        protected override void OnEnable()
        {
            this.selectionRectange = new Border()
            {
                Background = new SolidColorBrush(Color.FromArgb(0x22,  0x55, 0xFF, 0xFF)),
                BorderBrush = new SolidColorBrush(Color.FromArgb(0xCC, 0xAA, 0xFF, 0xFF)),
                BorderThickness = new Thickness(4),
                Visibility = Visibility.Collapsed,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            this.sceneObjectSelectionRectangle =
                Client.Scene.CreateSceneObject("Editor pointer tool - selection rectangle");

            Client.UI.AttachControl(
                      this.sceneObjectSelectionRectangle,
                      this.selectionRectange,
                      positionOffset: (0, 1),
                      isFocusable: true)
                  .UseWorldPositionZIndex = false;

            Panel.SetZIndex(this.selectionRectange, -1);
        }

        protected virtual void OnSelectionEnded()
        {
            this.selectionRectange.Visibility = Visibility.Collapsed;
        }

        protected virtual void OnSelectionExtended()
        {
            this.SelectionRectEndPosition = this.CurrentMouseTilePosition;
        }

        protected virtual void OnSelectionStarted()
        {
            this.SelectionRectStartPosition = this.SelectionRectEndPosition = this.CurrentMouseTilePosition;
            this.UpdateSelectionRectange();
        }

        protected virtual void OnUpdated()
        {
        }

        private void UpdateSelectionBounds()
        {
            var fromPosition = this.SelectionRectStartPosition;
            var toPosition = this.SelectionRectEndPosition;

            var minX = Math.Min(fromPosition.X, toPosition.X);
            var maxX = Math.Max(fromPosition.X, toPosition.X);

            var minY = Math.Min(fromPosition.Y, toPosition.Y);
            var maxY = Math.Max(fromPosition.Y, toPosition.Y);

            maxX++;
            maxY++;

            var bounds = new BoundsUshort(minX, minY, maxX, maxY);
            //Logger.WriteDev("Bounds: " + bounds);
            this.selectionBounds = this.ClampToWorldBounds(bounds);
            this.SelectionBoundsChanged?.Invoke();
        }

        private void UpdateSelectionRectange()
        {
            var bounds = this.SelectionBounds;

            this.sceneObjectSelectionRectangle.Position = (bounds.MinX, bounds.MaxY - 1);
            Vector2D size = (bounds.MaxX - bounds.MinX, bounds.MaxY - bounds.MinY);

            const int tileSize = ScriptingConstants.TileSizeVirtualPixels;
            this.selectionRectange.Width = tileSize * size.X;
            this.selectionRectange.Height = tileSize * size.Y;

            this.selectionRectange.Visibility = Visibility.Visible;
        }
    }
}