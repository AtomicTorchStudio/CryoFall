namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Actions;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.ClientOptions.General;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Systems.Cursor;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
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
        private static IStaticWorldObject currentInteractObject;

        private static IStaticWorldObject currentMouseOverObject;

        private ClientInputContext inputContext;

        public static IStaticWorldObject CurrentInteractObject
        {
            get => currentInteractObject;
            private set
            {
                if (currentInteractObject == value)
                {
                    return;
                }

                try
                {
                    currentInteractObject?.ProtoWorldObject.ClientInteractFinish(currentInteractObject);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex);
                }

                currentInteractObject = value;

                try
                {
                    currentInteractObject?.ProtoWorldObject.ClientInteractStart(currentInteractObject);
                }
                catch (Exception ex)
                {
                    Logger.Exception(ex);
                }

                if (currentInteractObject == null)
                {
                    return;
                }

                var currentAction = ClientCurrentCharacterHelper.PrivateState.CurrentActionState;
                if (currentAction?.TargetWorldObject == currentInteractObject)
                {
                    // interaction is in progress — don't finish it early
                    return;
                }

                try
                {
                    currentInteractObject.ProtoWorldObject.ClientInteractFinish(currentInteractObject);
                }
                finally
                {
                    currentInteractObject = null;
                }
            }
        }

        public static IStaticWorldObject CurrentMouseOverObject => currentMouseOverObject;

        public static bool CurrentMouseOverObjectIsOverClickArea
            => currentMouseOverObject != null
               && currentMouseOverObject
               == FindStaticObjectAtCurrentMousePosition(
                   Client.Characters.CurrentPlayerCharacter,
                   CollisionGroups.ClickArea);

        public static IStaticWorldObject FindStaticObjectAtCurrentMousePosition(
            ICharacter forCharacter,
            CollisionGroup collisionGroup)
        {
            var mouseWorldPosition = Api.Client.Input.MouseWorldPosition;
            var physicsSpace = forCharacter.PhysicsBody.PhysicsSpace;

            using (var objectsInTile = physicsSpace.TestPoint(mouseWorldPosition,
                                                              collisionGroup,
                                                              sendDebugEvent: false))
            {
                var list = objectsInTile.AsList();

                // find interactable static object
                for (var index = list.Count - 1; index >= 0; index--)
                {
                    var testResult = list[index];
                    var staticWorldObject = testResult.PhysicsBody.AssociatedWorldObject as IStaticWorldObject;
                    if (staticWorldObject?.ProtoStaticWorldObject is IInteractableProtoStaticWorldObject)
                    {
                        // return first found interactable static world object (latest added to tile if there are multiple objects)
                        return staticWorldObject;
                    }
                }

                // find non-interactable static object
                for (var index = list.Count - 1; index >= 0; index--)
                {
                    var testResult = list[index];
                    if (testResult.PhysicsBody.AssociatedWorldObject is IStaticWorldObject staticWorldObject)
                    {
                        // return first static world object (latest added to tile)
                        return staticWorldObject;
                    }
                }
            }

            return null;
        }

        public static void OnInteractionFinished(IWorldObject worldObject)
        {
            if (worldObject == null
                || CurrentInteractObject != worldObject)
            {
                return;
            }

            CurrentInteractObject = null;
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

        private static void SetCurrentMouseOverObjectWithClickArea(IStaticWorldObject value)
        {
            if (value == null
                && currentMouseOverObject == null)
            {
                // no need to update
                return;
            }

            if (currentMouseOverObject != value)
            {
                currentMouseOverObject?.ProtoStaticWorldObject.ClientObserving(
                    currentMouseOverObject,
                    isObserving: false);

                currentMouseOverObject = value;

                currentMouseOverObject?.ProtoStaticWorldObject.ClientObserving(
                    currentMouseOverObject,
                    isObserving: true);
            }

            if (currentMouseOverObject == null)
            {
                ClientCursorSystem.CurrentCursorId = CursorId.Default;
                InteractionTooltip.Hide();
                WorldObjectTitleTooltip.Hide();
                return;
            }

            if (!CurrentMouseOverObjectIsOverClickArea)
            {
                ClientCursorSystem.CurrentCursorId = CursorId.Default;
                WorldObjectTitleTooltip.Hide();
                return;
            }

            if (GeneralOptionDisplayObjectNameTooltip.IsDisplay)
            {
                var title = currentMouseOverObject.ProtoStaticWorldObject.ClientGetTitle(currentMouseOverObject);
                if (!string.IsNullOrEmpty(title))
                {
                    WorldObjectTitleTooltip.ShowOn(currentMouseOverObject, title);
                }
            }

            var canInteract = currentMouseOverObject.ProtoStaticWorldObject.SharedIsInsideCharacterInteractionArea(
                Client.Characters.CurrentPlayerCharacter,
                value,
                writeToLog: false);

            CursorId cursorId;

            if (currentMouseOverObject.ProtoStaticWorldObject
                    is IProtoStaticWorldObjectCustomInteractionCursor cursorProvider)
            {
                cursorId = cursorProvider.GetInteractionCursorId(canInteract);
            }
            else
            {
                cursorId = canInteract ? CursorId.InteractionPossible : CursorId.InteractionImpossible;
            }

            ClientCursorSystem.CurrentCursorId = cursorId;

            if (GeneralOptionDisplayObjectInteractionTooltip.IsDisplay
                && ClientComponentActionWithProgressWatcher.CurrentInteractionOverWorldObject
                != currentMouseOverObject)
            {
                var interactionTooltipText = currentMouseOverObject.ProtoStaticWorldObject.InteractionTooltipText;
                if (interactionTooltipText != null)
                {
                    InteractionTooltip.ShowOn(currentMouseOverObject,
                                              interactionTooltipText,
                                              canInteract);
                }
            }
        }

        private void Update()
        {
            var character = Api.Client.Characters.CurrentPlayerCharacter;
            if (character == null)
            {
                CurrentInteractObject = null;
                return;
            }

            var isCannotStartNewInteraction = WindowsManager.OpenedWindowsCount > 0
                                              || !MainMenuOverlay.IsHidden
                                              || Api.Client.UI.LayoutRoot.IsMouseOver;
            if (CurrentInteractObject == null
                && isCannotStartNewInteraction)
            {
                CurrentInteractObject = null;
                SetCurrentMouseOverObjectWithClickArea(null);
                return;
            }

            IStaticWorldObject mouseOverObjectCandidate;
            if (isCannotStartNewInteraction)
            {
                // cannot start new interaction - don't highlight the mouse over object
                mouseOverObjectCandidate = null;
            }
            else
            {
                // try to find static object at current mouse position
                // first try to find by click area
                mouseOverObjectCandidate = FindStaticObjectAtCurrentMousePosition(
                    character,
                    CollisionGroups.ClickArea);

                if (mouseOverObjectCandidate == null)
                {
                    // second try to find by default collider
                    mouseOverObjectCandidate = FindStaticObjectAtCurrentMousePosition(
                        character,
                        CollisionGroups.Default);
                }
            }

            SetCurrentMouseOverObjectWithClickArea(mouseOverObjectCandidate);

            // uncomment to restore "cancel action when the interact button released"
            //if (ClientInputManager.IsButtonUp(GameButton.ActionInteract))
            //{
            //    // world object interaction button released
            //    CurrentInteractObject = null;
            //    return;
            //}

            if (!ClientInputManager.IsButtonDown(GameButton.ActionInteract))
            {
                if (CurrentInteractObject == null)
                {
                    return;
                }

                // have a current interaction - check whether it's not valid anymore
                if (!CurrentInteractObject.ProtoStaticWorldObject.SharedIsInsideCharacterInteractionArea(
                        Client.Characters.CurrentPlayerCharacter,
                        CurrentInteractObject,
                        writeToLog: false))
                {
                    Logger.Important(
                        $"Cannot interact with {nameof(CurrentInteractObject)}: {CurrentInteractObject} - finishing interaction");
                    CurrentInteractObject = null;
                }

                return;
            }

            if (CurrentInteractObject == null)
            {
                // world object interaction button pressed
                CurrentInteractObject = CurrentMouseOverObject;
            }
            else
            {
                // already interacting with something
                if (CurrentMouseOverObject == null)
                {
                    // no current interaction
                    CurrentInteractObject = null;
                }
                else if (CurrentMouseOverObject == CurrentInteractObject)
                {
                    // clicked again on the same object as the current interaction - cancel it
                    CurrentInteractObject = null;
                }
                else
                {
                    // change to interact with the current pointed object
                    CurrentInteractObject = CurrentMouseOverObject;
                }
            }
        }
    }
}