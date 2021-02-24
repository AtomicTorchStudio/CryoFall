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
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
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
            using var tempList = Api.Shared.WrapInTempList(
                FindObjectsAtCurrentMousePosition(forCharacter, collisionGroup));

            switch (tempList.Count)
            {
                case 0:
                    return null;

                case 1:
                    return tempList.AsList()[0];

                default:
                    tempList.AsList()
                            .Sort(ComparebByPositionY);
                    return tempList.AsList()[0];
            }

            static int ComparebByPositionY(IWorldObject x, IWorldObject y)
            {
                var xDynamic = x as IDynamicWorldObject;
                var yDynamic = y as IDynamicWorldObject;

                if (xDynamic is not null
                    && yDynamic is not null)
                {
                    return xDynamic.Position.Y
                                   .CompareTo(yDynamic.Position.Y);
                }

                if (xDynamic is not null)
                {
                    // dynamic objects have priority over static objects
                    return -1;
                }

                if (yDynamic is not null)
                {
                    // dynamic objects have priority over static objects
                    return 1;
                }

                var result = (y.ProtoGameObject is ObjectPlayerLootContainer)
                    .CompareTo(x.ProtoGameObject is ObjectPlayerLootContainer);
                if (result != 0)
                {
                    // player loot has priority over other static objects 
                    return result;
                }

                result = ((IStaticWorldObject)y).ProtoStaticWorldObject
                                                .Kind
                                                .CompareTo(((IStaticWorldObject)x).ProtoStaticWorldObject
                                                           .Kind);

                if (result != 0)
                {
                    return result;
                }

                result = x.TilePosition.Y
                          .CompareTo(y.TilePosition.Y);

                if (result != 0)
                {
                    return result;
                }

                // order by date of creation
                return y.Id.CompareTo(x.Id);
            }
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
                                                  .HandleAll(Update);
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
                || ClientCurrentCharacterHelper.Character.GetPublicState<PlayerCharacterPublicState>().CurrentVehicle
                    is not null)
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

            var canInteract = MouseOverObject.ProtoWorldObject.SharedCanInteract(
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
                if (interactionTooltipText is not null)
                {
                    InteractionTooltip.ShowOn(MouseOverObject,
                                              interactionTooltipText,
                                              canInteract);
                }
            }
        }

        private static void Update()
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
                    if (mouseOverObjectCandidate is null)
                    {
                        // take the last static object in the tile (only if it has no physics body)
                        var tile = Api.Client.World.GetTile(Api.Client.Input.MousePointedTilePosition);
                        if (tile.IsValidTile)
                        {
                            mouseOverObjectCandidate = tile.StaticObjects.LastOrDefault(
                                st => st.PhysicsBody is null
                                      || !st.PhysicsBody.HasShapes);
                        }
                    }
                }
            }

            if (mouseOverObjectCandidate is not null
                && (ConstructionPlacementSystem.IsInObjectPlacementMode
                    || ConstructionRelocationSystem.IsInObjectPlacementMode)
                && !(mouseOverObjectCandidate.ProtoGameObject is ProtoObjectConstructionSite))
            {
                // cannot observe already built structures while in placement selection mode
                mouseOverObjectCandidate = null;
            }

            SetCurrentMouseOverObject(mouseOverObjectCandidate);

            if (character.GetPublicState<PlayerCharacterPublicState>().CurrentVehicle is not null)
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
                // no need to perform any distance checks here as InteractionCheckerSystem will do them
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