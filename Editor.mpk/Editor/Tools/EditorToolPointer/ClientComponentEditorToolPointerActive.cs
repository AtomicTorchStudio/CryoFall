namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolPointer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentEditorToolPointerActive : BaseClientComponentEditorToolSelectLocation
    {
        private readonly List<IStaticWorldObject> lastStaticWorldObjectsAtMousePointedTile
            = new();

        private readonly HashSet<IStaticWorldObject> selectedWorldObjects = new();

        private Action<Vector2Ushort> clientPasteCallback;

        private Action<IReadOnlyCollection<IStaticWorldObject>> copyCallback;

        private Action<IReadOnlyCollection<IStaticWorldObject>> deleteCallback;

        private bool isInitialized;

        private ClientComponentEditorToolPointerActiveSelectionDisplay selectionComponent;

        private TextBlock textBlock;

        private Border textBlockPanel;

        public IReadOnlyCollection<IStaticWorldObject> SelectedWorldObjects => this.selectedWorldObjects;

        public void Setup(
            Action<IReadOnlyCollection<IStaticWorldObject>> deleteCallback,
            Action<IReadOnlyCollection<IStaticWorldObject>> copyCallback,
            Action<Vector2Ushort> clientPasteCallback)
        {
            this.deleteCallback = deleteCallback;
            this.copyCallback = copyCallback;
            this.clientPasteCallback = clientPasteCallback;
        }

        public override void Update(double deltaTime)
        {
            if (ClientEditorAreaSelectorHelper.Instance is not null)
            {
                // in an area selection mode
                this.OnSelectionEnded();
                return;
            }

            base.Update(deltaTime);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (this.isInitialized)
            {
                return;
            }

            this.isInitialized = true;

            this.selectionComponent = this
                                      .SceneObject
                                      .AddComponent<ClientComponentEditorToolPointerActiveSelectionDisplay>();

            this.textBlock = new TextBlock()
            {
                Foreground = new SolidColorBrush(Color.FromArgb(0xEE, 0xFF, 0xFF, 0xFF)),
                FontSize = 13
            };

            this.textBlockPanel = new Border()
            {
                Child = this.textBlock,
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(5, 3, 5, 3),
                Margin = new Thickness(5),
                Background = new SolidColorBrush(Color.FromArgb(0x77, 0, 0, 0)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Visibility = Visibility.Hidden
            };

            Client.UI.AttachControl(
                this.SceneObject,
                this.textBlockPanel,
                positionOffset: (1, 0),
                isScaleWithCameraZoom: false,
                isFocusable: false);

            Panel.SetZIndex(this.textBlockPanel, 100);
        }

        protected override void OnSelectionEnded()
        {
            base.OnSelectionEnded();
            this.RefreshSelection();
        }

        protected override void OnSelectionStarted()
        {
            base.OnSelectionStarted();
            this.RefreshSelection();
        }

        protected override void OnUpdated()
        {
            base.OnUpdated();

            if (!WorldService.WorldBounds.Contains(this.CurrentMouseTilePosition))
            {
                return;
            }

            var tile = WorldService.GetTile(this.CurrentMouseTilePosition);
            var staticWorldObjects = tile.StaticObjects;
            this.RefreshTooltip(staticWorldObjects);

            if (ClientInputManager.IsButtonDown(EditorButton.EditorDeleteSelectedObjects))
            {
                this.deleteCallback(this.SelectedWorldObjects);
                return;
            }

            var input = Api.Client.Input;
            if (input.IsKeyHeld(InputKey.Control, evenIfHandled: true))
            {
                if (input.IsKeyDown(InputKey.C))
                {
                    // Ctrl+C
                    input.ConsumeKey(InputKey.C);
                    this.copyCallback(this.SelectedWorldObjects);
                }
                else if (input.IsKeyDown(InputKey.X))
                {
                    // Ctrl+X
                    input.ConsumeKey(InputKey.X);
                    this.copyCallback(this.SelectedWorldObjects);
                    this.deleteCallback(this.SelectedWorldObjects);
                }
                else if (input.IsKeyDown(InputKey.V))
                {
                    // Ctrl+V
                    input.ConsumeKey(InputKey.V);
                    this.clientPasteCallback(this.CurrentMouseTilePosition);
                }
            }

            this.selectedWorldObjects.RemoveWhere(
                worldObject =>
                {
                    if (!worldObject.IsDestroyed)
                    {
                        return false;
                    }

                    this.selectionComponent.Deselect(worldObject);
                    return true;
                });
        }

        private IReadOnlyCollection<IStaticWorldObject> GetWorldObjectsInRectangle()
        {
            var bounds = this.SelectionBounds;
            return WorldService.GetStaticObjectsAtBounds(bounds);
        }

        private void RefreshSelection()
        {
            var worldObjects = this.GetWorldObjectsInRectangle();

            var input = Api.Client.Input;
            var isAddMode = input.IsKeyHeld(InputKey.Shift,      evenIfHandled: true)
                            || input.IsKeyHeld(InputKey.Control, evenIfHandled: true);
            var isRemoveMode = input.IsKeyHeld(InputKey.Alt, evenIfHandled: true);

            if (!isAddMode
                && !isRemoveMode)
            {
                // reset current selection
                if (worldObjects.SequenceEqual(this.selectedWorldObjects))
                {
                    return;
                }

                foreach (var previouslySelectedWorldObject in this.selectedWorldObjects)
                {
                    this.selectionComponent.Deselect(previouslySelectedWorldObject);
                }

                this.selectedWorldObjects.Clear();
                this.selectedWorldObjects.AddRange(worldObjects);

                foreach (var obj in worldObjects)
                {
                    this.selectionComponent.Select(obj);
                }
            }
            else if (isAddMode)
            {
                foreach (var obj in worldObjects)
                {
                    if (this.selectedWorldObjects.Add(obj))
                    {
                        this.selectionComponent.Select(obj);
                    }
                }
            }
            else if (isRemoveMode)
            {
                foreach (var obj in worldObjects)
                {
                    if (this.selectedWorldObjects.Remove(obj))
                    {
                        this.selectionComponent.Deselect(obj);
                    }
                }
            }
        }

        private void RefreshTooltip(ReadOnlyListWrapper<IStaticWorldObject> staticWorldObjects)
        {
            if (staticWorldObjects.SequenceEqual(this.lastStaticWorldObjectsAtMousePointedTile))
            {
                return;
            }

            this.lastStaticWorldObjectsAtMousePointedTile.Clear();
            this.lastStaticWorldObjectsAtMousePointedTile.AddRange(staticWorldObjects);

            if (this.lastStaticWorldObjectsAtMousePointedTile.Count > 0)
            {
                this.textBlock.Text =
                    staticWorldObjects.Reverse()
                                      .Select(o => $"{o.ProtoStaticWorldObject.ShortId} ID={o.Id}")
                                      .GetJoinedString(Environment.NewLine)
                                      .ToString();
                this.textBlockPanel.Visibility = Visibility.Visible;
            }
            else
            {
                this.textBlockPanel.Visibility = Visibility.Hidden;
            }
        }
    }
}