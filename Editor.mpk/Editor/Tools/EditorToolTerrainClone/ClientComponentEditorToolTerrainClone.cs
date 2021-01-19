namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolTerrainClone
{
    using System;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentEditorToolTerrainClone : ClientComponentEditorToolSelectLocationWithExtending
    {
        private Action<Vector2Ushort> clientPasteCallback;

        private Action<BoundsUshort> copyCallback;

        public void Setup(
            Action<BoundsUshort> copyCallback,
            Action<Vector2Ushort> clientPasteCallback)
        {
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

        protected override void OnUpdated()
        {
            base.OnUpdated();

            if (!WorldService.WorldBounds.Contains(this.CurrentMouseTilePosition))
            {
                return;
            }

            var input = Api.Client.Input;
            if (!input.IsKeyHeld(InputKey.Control, evenIfHandled: true))
            {
                return;
            }

            if (input.IsKeyDown(InputKey.C)
                && this.SelectionBounds.Size != default)
            {
                // Ctrl+C
                input.ConsumeKey(InputKey.C);
                this.copyCallback(this.SelectionBounds);
                this.SetSelectionBounds(default);
            }
            else if (input.IsKeyDown(InputKey.V))
            {
                // Ctrl+V
                input.ConsumeKey(InputKey.V);
                this.clientPasteCallback(this.SelectionBounds.Size != default
                                             ? this.SelectionBounds.Offset
                                             : Client.Input.MouseWorldPosition.ToVector2Ushort());
                this.SetSelectionBounds(default);
            }
        }
    }
}