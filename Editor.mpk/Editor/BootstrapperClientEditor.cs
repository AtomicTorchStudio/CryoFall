namespace AtomicTorch.CBND.CoreMod.Editor
{
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Camera;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Core;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Editor.Data;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolMap;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class BootstrapperClientEditor : BaseBootstrapper
    {
        private static readonly Interval<double> ZoomBoundsEditorMode
            = new Interval<double>(
                min: 0.05,
                // please note - in Editor mode we allow to zoom closer (to check HiDPI sprites)
                max: Api.IsEditor ? 8 : 1);

        private ClientInputContext inputContextEditorMapMenu;

        private ClientInputContext inputContextEditorUndoRedo;

        public override void ClientInitialize()
        {
            ClientInputManager.RegisterButtonsEnum<EditorButton>();

            ClientComponentUpdateHelper.UpdateCallback += this.ClientUpdateCallback;
            BootstrapperClientGame.InitEditorModeCallback += this.InitEditorMode;
            BootstrapperClientGame.ResetCallback += this.Reset;

            ClientInputContext.Start("Editor quick save/load")
                              .HandleButtonDown(
                                  EditorButton.LoadQuickSavegame,
                                  EditorToolMap.ClientLoadSavegameQuick)
                              .HandleButtonDown(
                                  EditorButton.MakeQuickSavegame,
                                  EditorToolMap.ClientSaveSavegameQuick);
        }

        private void ClientUpdateCallback()
        {
            if (!ClientInputManager.IsButtonDown(EditorButton.ToggleEditorMode))
            {
                return;
            }

            var currentPlayerCharacter = Client.Characters.CurrentPlayerCharacter;
            if (currentPlayerCharacter == null)
            {
                return;
            }

            // reset focus (workaround for NoesisGUI crash when not focused unloaded control receives OnKeyDown)
            Client.UI.BlurFocus();

            var protoCharacterEditorMode = Api.GetProtoEntity<PlayerCharacterEditorMode>();
            if (currentPlayerCharacter.ProtoCharacter is PlayerCharacterEditorMode)
            {
                // switch to player mode
                EditorActiveToolManager.Deactivate();
                protoCharacterEditorMode
                    .CallServer(_ => _.ServerRemote_SwitchToPlayerMode());
            }
            else
            {
                // switch to editor mode
                protoCharacterEditorMode
                    .CallServer(_ => _.ServerRemote_SwitchToEditorMode());
            }
        }

        private void InitEditorMode(ICharacter currentCharacter)
        {
            Api.Client.UI.LayoutRootChildren.Add(new EditorHUDLayoutControl());

            ClientComponentWorldCameraZoomManager.Instance.ZoomBounds = ZoomBoundsEditorMode;
            Menu.Register<WindowEditorWorldMap>();

            this.inputContextEditorMapMenu
                = ClientInputContext
                  .Start("Editor map")
                  .HandleButtonDown(
                      GameButton.MapMenu,
                      Menu.Toggle<WindowEditorWorldMap>);

            this.inputContextEditorUndoRedo
                = ClientInputContext
                  .Start("Editor undo/redo")
                  .HandleAll(
                      () =>
                      {
                          var input = Client.Input;
                          if (input.IsKeyDown(InputKey.Z)
                              && input.IsKeyHeld(InputKey.Control))
                          {
                              if (input.IsKeyHeld(InputKey.Shift))
                              {
                                  EditorClientSystem.Redo();
                                  return;
                              }

                              EditorClientSystem.Undo();
                              return;
                          }

                          if (input.IsKeyDown(InputKey.Y)
                              && input.IsKeyHeld(InputKey.Control))
                          {
                              EditorClientSystem.Redo();
                              return;
                          }
                      });
        }

        private void Reset()
        {
            if (EditorHUDLayoutControl.Instance != null)
            {
                Api.Client.UI.LayoutRootChildren.Remove(EditorHUDLayoutControl.Instance);
            }

            this.inputContextEditorMapMenu?.Stop();
            this.inputContextEditorMapMenu = null;

            this.inputContextEditorUndoRedo?.Stop();
            this.inputContextEditorUndoRedo = null;
        }
    }
}