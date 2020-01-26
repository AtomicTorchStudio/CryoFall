namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Actions;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.ClientOptions.General;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Systems.Cursor;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu;
    using AtomicTorch.CBND.CoreMod.UI.Services;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;

    public class ClientComponentObjectInteractionHelper : ClientComponent
    {
        /// <summary>
        /// Current object with which the client is interacting (holding right mouse button).
        /// </summary>
        private static IWorldObject interactingWithObject;

        private ClientInputContext inputContext;

        public static IWorldObject InteractingWithObject
        {
            get => interactingWithObject;
            private set
            {
                if (interactingWithObject == value)
                {
                    return;
                }

                try
                {
                    interactingWithObject?.ProtoWorldObject.ClientInteractFinish(interactingWithObject);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex);
                }

                interactingWithObject = value;

                try
                {
                    interactingWithObject?.ProtoWorldObject.ClientInteractStart(interactingWithObject);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex);
                }

                if (interactingWithObject is null)
                {
                    return;
                }

                var currentAction = ClientCurrentCharacterHelper.PrivateState.CurrentActionState;
                if (currentAction?.TargetWorldObject == interactingWithObject)
                {
                    // interaction is in progress — don't finish it early
                    return;
                }

                try
                {
                    interactingWithObject.ProtoWorldObject.ClientInteractFinish(interactingWithObject);
                }
                finally
                {
                    interactingWithObject = null;
                }
            }
        }

        public static IWorldObject MouseOverObject { get; private set; }

        public static IWorldObject FindObjectAtCurrentMousePosition(
            ICharacter forCharacter,
            CollisionGroup collisionGroup)
        {
            return FindObjectsAtCurrentMousePosition(forCharacter, collisionGroup)
                .FirstOrDefault();
        }

        public static IEnumerable<IWorldObject> FindObjectsAtCurrentMousePosition(
            ICharacter forCharacter,
            CollisionGroup collisionGroup)
        {
            var mouseWorldPosition = Api.Client.Input.MouseWorldPosition;
            var physicsSpace = forCharacter.PhysicsBody.PhysicsSpace;

            using var objectsInTile = physicsSpace.TestPoint(mouseWorldPosition,
                                                             collisionGroup,
                                                             sendDebugEvent: false);
            var list = objectsInTile.AsList();

            // find interactable object (can be dynamic or static object)
            for (var index = list.Count - 1; index >= 0; index--)
            {
                var testResult = list[index];
                var worldObject = testResult.PhysicsBody.AssociatedWorldObject;
                if (worldObject == forCharacter)
                {
                    continue;
                }

                if (worldObject?.ProtoWorldObject is IInteractableProtoWorldObject interactableProtoWorldObject
                    && interactableProtoWorldObject.IsInteractableObject)
                {
                    // return first found interactable static world object (latest added to tile if there are multiple objects)
                    yield return worldObject;
                }
            }

            // find non-interactable object (can be only static)
            for (var index = list.Count - 1; index >= 0; index--)
            {
                var testResult = list[index];
                var worldObject = testResult.PhysicsBody.AssociatedWorldObject;
                if (worldObject == forCharacter)
                {
                    continue;
                }

                if (worldObject is IStaticWorldObject)
                {
                    // return first world object (latest added to tile)
                    yield return worldObject;
                }
            }
        }

        public static void OnInteractionFinished(IWorldObject worldObject)
        {
            if (worldObject is null
                || InteractingWithObject != worldObject)
            {
                return;
            }

            InteractingWithObject = null;
        }

        protected override void OnDisable()
        {
            this.inputContext.Stop();
            this.inputContext = null;
        }

        protected override void OnEnable()
        {
            this.inputContext = ClientInputContext.Start(nameof(ClientComponentObjectInteractionHelper))
                                                  .HandleAll(this.Update);
        }

        private static void SetCurrentMouseOverObject(IWorldObject value)
        {
            if (value is null
                && MouseOverObject is null)
            {
                // no need to update
                return;
            }

            if (MouseOverObject != value)
            {
                MouseOverObject?.ProtoWorldObject.ClientObserving(
                    MouseOverObject,
                    isObserving: false);

                MouseOverObject = value;

                MouseOverObject?.ProtoWorldObject.ClientObserving(
                    MouseOverObject,
                    isObserving: true);
            }

            if (MouseOverObject is null
                // cannot interact while on vehicle
                || ClientCurrentCharacterHelper.Character.GetPublicState<PlayerCharacterPublicState>().CurrentVehicle != null)
            {
                ClientCursorSystem.CurrentCursorId = CursorId.Default;
                InteractionTooltip.Hide();
                WorldObjectTitleTooltip.Hide();
                return;
            }

            if (!MouseOverObject.ProtoWorldObject.IsInteractableObject
                || (MouseOverObject
                    != FindObjectAtCurrentMousePosition(
                        Client.Characters.CurrentPlayerCharacter,
                        CollisionGroups.ClickArea)))
            {
                ClientCursorSystem.CurrentCursorId = CursorId.Default;
                WorldObjectTitleTooltip.Hide();
                return;
            }

            if (GeneralOptionDisplayObjectNameTooltip.IsDisplay)
            {
                var title = MouseOverObject.ProtoWorldObject.ClientGetTitle(MouseOverObject);
                if (!string.IsNullOrEmpty(title))
                {
                    WorldObjectTitleTooltip.ShowOn(MouseOverObject, title);
                }
            }

            var canInteract = MouseOverObject.ProtoWorldObject.SharedIsInsideCharacterInteractionArea(
                Client.Characters.CurrentPlayerCharacter,
                value,
                writeToLog: false);

            CursorId cursorId;

            if (MouseOverObject.ProtoWorldObject
                    is IProtoWorldObjectCustomInteractionCursor cursorProvider)
            {
                cursorId = cursorProvider.GetInteractionCursorId(canInteract);
            }
            else
            {
                cursorId = canInteract
                               ? CursorId.InteractionPossible
                               : CursorId.InteractionImpossible;
            }

            ClientCursorSystem.CurrentCursorId = cursorId;

            if (GeneralOptionDisplayObjectInteractionTooltip.IsDisplay
                && (ClientComponentActionWithProgressWatcher.CurrentInteractionOverWorldObject
                    != MouseOverObject))
            {
                var interactionTooltipText = MouseOverObject.ProtoWorldObject.InteractionTooltipText;
                if (interactionTooltipText != null)
                {
                    InteractionTooltip.ShowOn(MouseOverObject,
                                              interactionTooltipText,
                                              canInteract);
                }
            }
        }

        private void Update()
        {
            var character = Api.Client.Characters.CurrentPlayerCharacter;
            if (character is null)
            {
                InteractingWithObject = null;
                return;
            }

            var isCannotStartNewInteraction = WindowsManager.OpenedWindowsCount > 0
                                              || !MainMenuOverlay.IsHidden
                                              || Api.Client.UI.LayoutRoot.IsMouseOver;

            if (InteractingWithObject is null
                && isCannotStartNewInteraction)
            {
                InteractingWithObject = null;
                SetCurrentMouseOverObject(null);
                return;
            }

            IWorldObject mouseOverObjectCandidate;
            if (isCannotStartNewInteraction)
            {
                // cannot start new interaction - don't highlight the mouse over object
                mouseOverObjectCandidate = null;
            }
            else
            {
                // try to find static object at current mouse position
                // first try to find by click area
                mouseOverObjectCandidate = FindObjectAtCurrentMousePosition(character, CollisionGroups.ClickArea);

                if (mouseOverObjectCandidate is null)
                {
                    // second try to find by default collider
                    mouseOverObjectCandidate = FindObjectAtCurrentMousePosition(character, CollisionGroups.Default);
                }
            }

            SetCurrentMouseOverObject(mouseOverObjectCandidate);

            if (character.GetPublicState<PlayerCharacterPublicState>().CurrentVehicle != null)
            {
                // cannot interact while on vehicle
                InteractingWithObject = null;
                return;
            }

            // uncomment to restore "cancel action when the interact button released"
            //if (ClientInputManager.IsButtonUp(GameButton.ActionInteract))
            //{
            //    // world object interaction button released
            //    CurrentInteractObject = null;
            //    return;
            //}

            if (!ClientInputManager.IsButtonDown(GameButton.ActionInteract))
            {
                if (InteractingWithObject is null)
                {
                    return;
                }

                // have a current interaction - check whether it's not valid anymore
                if (!InteractingWithObject.ProtoWorldObject.SharedIsInsideCharacterInteractionArea(
                        Client.Characters.CurrentPlayerCharacter,
                        InteractingWithObject,
                        writeToLog: false))
                {
                    Logger.Important(
                        $"Cannot interact with {nameof(InteractingWithObject)}: {InteractingWithObject} - finishing interaction");
                    InteractingWithObject = null;
                }

                return;
            }

            if (InteractingWithObject is null)
            {
                // world object interaction button pressed
                InteractingWithObject = MouseOverObject;
            }
            else
            {
                // already interacting with something
                if (MouseOverObject is null)
                {
                    // no current interaction
                    InteractingWithObject = null;
                }
                else if (MouseOverObject == InteractingWithObject)
                {
                    // clicked again on the same object as the current interaction - cancel it
                    InteractingWithObject = null;
                }
                else if (MouseOverObject.ProtoWorldObject.IsInteractableObject)
                {
                    // change to interact with the current pointed object
                    InteractingWithObject = MouseOverObject;
                }
            }
        }
    }
}