namespace AtomicTorch.CBND.CoreMod.Editor.Tools.Brushes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Editor.Scripts.Helpers;
    using AtomicTorch.CBND.CoreMod.Editor.Tools.Base;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public class EditorActiveToolObjectBrush : BaseEditorActiveTool
    {
        private static readonly IInputClientService Input = Api.Client.Input;

        private readonly ClientInputContext inputContext;

        private readonly Action onDispose;

        private readonly Action<List<Vector2Ushort>> onSelected;

        private readonly IProtoStaticWorldObject protoStaticObject;

        private readonly IClientSceneObject sceneObject;

        private uint lastJumpObjectId;

        public EditorActiveToolObjectBrush(
            IProtoStaticWorldObject protoStaticObject,
            [NotNull] Action<List<Vector2Ushort>> onSelected,
            Action onDispose = null)
        {
            this.protoStaticObject = protoStaticObject;
            this.onSelected = onSelected;
            this.onDispose = onDispose;

            this.sceneObject = Api.Client.Scene.CreateSceneObject("ActiveEditorToolBrush", Vector2D.Zero);
            this.sceneObject.AddComponent<ClientComponentObjectPlacementHelper>()
                .Setup(
                    protoStaticObject,
                    isCancelable: false,
                    isRepeatCallbackIfHeld: true,
                    isDrawConstructionGrid: false,
                    isBlockingInput: false,
                    validateCanPlaceCallback: this.ValidateCanBuild,
                    placeSelectedCallback: this.PlaceSelectedHandler);

            this.inputContext
                = ClientInputContext
                  .Start(nameof(EditorActiveToolObjectBrush))
                  .HandleAll(
                      () =>
                      {
                          if (Input.IsKeyHeld(InputKey.MouseRightButton, evenIfHandled: false))
                          {
                              Input.ConsumeKey(InputKey.MouseRightButton);
                              DeleteObjectsUnderMouseCursor();
                          }
                          else if (Input.IsKeyDown(InputKey.Tab, evenIfHandled: false))
                          {
                              Input.ConsumeKey(InputKey.Tab);
                              Api.Client.UI.BlurFocus();
                              this.JumpToNextObject(protoStaticObject);
                          }
                      });
        }

        public override void Dispose()
        {
            this.inputContext.Stop();
            this.sceneObject.Destroy();
            this.onDispose?.Invoke();
        }

        private static void DeleteObjectsUnderMouseCursor()
        {
            var worldObjectsToDelete = Api.Client.World.GetTile(
                                              Input.MousePointedTilePosition)
                                          .StaticObjects;

            if (worldObjectsToDelete.Count > 0)
            {
                EditorStaticObjectsRemovalHelper.ClientDelete(
                    worldObjectsToDelete.ToList());
            }
        }

        private void JumpToNextObject(IProtoStaticWorldObject protoStaticObject)
        {
            using var objectsOfProto =
                Api.Shared.WrapInTempList(
                    Api.Client.World.GetStaticWorldObjectsOfProto(protoStaticObject));

            if (objectsOfProto.Count == 0)
            {
                return;
            }

            objectsOfProto.SortBy(o => o.Id);
            var isReverseMode = false;
            if (Input.IsKeyHeld(InputKey.Shift, evenIfHandled: true))
            {
                isReverseMode = true;
                objectsOfProto.AsList().Reverse();
            }

            foreach (var worldObject in objectsOfProto.AsList())
            {
                if (isReverseMode)
                {
                    if (worldObject.Id >= this.lastJumpObjectId)
                    {
                        continue;
                    }
                }
                else if (worldObject.Id <= this.lastJumpObjectId)
                {
                    continue;
                }

                // jump to it
                JumpTo(worldObject);
                return;
            }

            JumpTo(objectsOfProto.AsList()[0]);

            void JumpTo(IStaticWorldObject staticWorldObject)
            {
                this.lastJumpObjectId = staticWorldObject.Id;
                var center = staticWorldObject.ProtoStaticWorldObject
                                              .Layout.Center;
                Api.Client.World.SetPosition(
                    ClientCurrentCharacterHelper.Character,
                    position: staticWorldObject.TilePosition.ToVector2D()
                              + (center.X, center.Y),
                    forceReset: true);
            }
        }

        private void PlaceSelectedHandler(Vector2Ushort tilePosition)
        {
            this.onSelected(
                new List<Vector2Ushort>()
                {
                    tilePosition
                });
        }

        private void ValidateCanBuild(
            Vector2Ushort tilePosition,
            bool logErrors,
            out string errorMessage,
            out bool canPlace,
            out bool isTooFar)
        {
            isTooFar = false;
            canPlace = this.protoStaticObject.CheckTileRequirements(tilePosition,
                                                                    character: null,
                                                                    out var errorCodeOrMessage,
                                                                    logErrors: logErrors);
            
            errorMessage = ConstructionSystem.SharedConvertCodeOrErrorMessageToString(errorCodeOrMessage);
        }
    }
}