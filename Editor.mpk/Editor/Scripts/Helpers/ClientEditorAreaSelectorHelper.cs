namespace AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers
{
    using System;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public class ClientEditorAreaSelectorHelper
    {
        private readonly ClientComponentEditorToolSelectLocationFixedSize componentSelectLocation;

        private readonly Action<Vector2Ushort> selectedCallback;

        private readonly Vector2Ushort size;

        private IClientSceneObject sceneObject;

        public ClientEditorAreaSelectorHelper(
            Vector2Ushort tilePosition,
            Vector2Ushort size,
            Action<Vector2Ushort> selectedCallback)
        {
            Instance = this;
            this.size = size;
            this.selectedCallback = selectedCallback;

            this.sceneObject = Api.Client.Scene.CreateSceneObject("Editor Generator Tool");
            this.componentSelectLocation =
                this.sceneObject.AddComponent<ClientComponentEditorToolSelectLocationFixedSize>();

            this.componentSelectLocation.SetSelectionBounds(
                new BoundsUshort(offset: tilePosition,
                                 size: size));

            var selectionRectange = this.componentSelectLocation.SelectionRectange;
            selectionRectange.Background = new SolidColorBrush(Color.FromArgb(0x22,  0xDD, 0x00, 0x00));
            selectionRectange.BorderBrush = new SolidColorBrush(Color.FromArgb(0xCC, 0xDD, 0x00, 0x00));

            ClientUpdateHelper.UpdateCallback += this.Update;
        }

        [CanBeNull]
        public static ClientEditorAreaSelectorHelper Instance { get; private set; }

        public void Destroy()
        {
            if (this.sceneObject is null)
            {
                throw new Exception("Already destroyed");
            }

            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }

            this.sceneObject.Destroy();
            this.sceneObject = null;

            ClientUpdateHelper.UpdateCallback -= this.Update;
        }

        private bool CheckIsValidOffset(Vector2Ushort offset)
        {
            var worldBounds = Api.Client.World.WorldBounds;
            var maxPosition = offset + this.size;

            if (maxPosition.X < worldBounds.MaxX
                && maxPosition.Y < worldBounds.MaxY
                && offset.X > worldBounds.MinX
                && offset.Y > worldBounds.MinY)
            {
                return true;
            }

            NotificationSystem.ClientShowNotification(
                title: null,
                message:
                "Out of world bounds, please select different location",
                color: NotificationColor.Bad);
            return false;
        }

        private void Update()
        {
            var input = Api.Client.Input;
            if (input.IsKeyDown(InputKey.Z)
                && input.IsKeyHeld(InputKey.Control))
            {
                input.ConsumeKey(InputKey.Z);
                this.Destroy();
                return;
            }

            if (input.IsKeyDown(InputKey.Escape))
            {
                input.ConsumeKey(InputKey.Escape);
                this.Destroy();
                return;
            }

            if (input.IsKeyDown(InputKey.Enter))
            {
                input.ConsumeKey(InputKey.Enter);
                var offset = this.componentSelectLocation.SelectionBounds.Offset;

                if (this.CheckIsValidOffset(offset))
                {
                    this.Destroy();
                    this.selectedCallback(offset);
                }
            }
        }
    }
}