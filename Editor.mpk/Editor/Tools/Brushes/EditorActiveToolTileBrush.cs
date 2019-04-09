namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public class EditorActiveToolTileBrush : BaseEditorActiveTool
    {
        protected readonly ClientComponentEditorToolActiveTileBrush Component;

        protected readonly IClientSceneObject SceneObject;

        private readonly Action onDispose;

        public EditorActiveToolTileBrush(
            [NotNull] ApplyToolDelegate onSelected,
            Func<List<Vector2Ushort>, bool> validateCallback = null,
            Action onDispose = null,
            bool isRepeatOnMove = true,
            bool isReactOnRightMouseButton = false,
            [CanBeNull] Action onReleaseInput = null)
        {
            this.onDispose = onDispose;

            this.SceneObject = Api.Client.Scene.CreateSceneObject("ActiveEditorToolBrush", Vector2D.Zero);
            this.Component = this.SceneObject.AddComponent<ClientComponentEditorToolActiveTileBrush>();
            this.Component.Setup(onSelected, validateCallback, isRepeatOnMove, inputReleasedCallback: onReleaseInput);
            this.Component.IsReactOnRightMouseButton = isReactOnRightMouseButton;
        }

        public EditorBrushShape Brush { get; private set; } = EditorBrushShape.Circle;

        public int BrushSize { get; private set; } = 1;

        public override void Dispose()
        {
            this.SceneObject.Destroy();
            this.onDispose?.Invoke();
        }

        public void SetBrush(EditorBrushShape brush, int brushSize)
        {
            this.Brush = brush;
            this.BrushSize = brushSize;
            this.Component.SetBrush(brush, brushSize);
        }

        public void SetCustomBrush(params Vector2Int[] offsets)
        {
            this.Component.SetCustomBrush(offsets);
        }
    }
}