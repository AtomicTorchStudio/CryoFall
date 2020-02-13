namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    internal static class StructureLandClaimIndicatorManager
    {
        private static readonly Dictionary<IStaticWorldObject, StructureLandClaimIndicator>
            initializedOverlays
                = new Dictionary<IStaticWorldObject, StructureLandClaimIndicator>(capacity: 128);

        private static IComponentAttachedControl lastHoverIndicator;

        static StructureLandClaimIndicatorManager()
        {
            ClientUpdateHelper.UpdateCallback += Update;
        }

        public static void ClientDeinitialize(IStaticWorldObject worldObject)
        {
            if (!initializedOverlays.TryGetValue(worldObject, out var control))
            {
                return;
            }

            initializedOverlays.Remove(worldObject);
            if (control is null)
            {
                return;
            }

            DestroyControl(control);
        }

        public static void ClientInitialize(IStaticWorldObject worldObject)
        {
            // ensure there is no existing entry for it
            ClientDeinitialize(worldObject);

            var isLandClaimedByAnyone = LandClaimSystem.SharedIsObjectInsideAnyArea(worldObject);
            StructureLandClaimIndicator control = null;

            if (!isLandClaimedByAnyone)
            {
                var component = SetupFor(worldObject, isClaimed: false);
                control = (StructureLandClaimIndicator)component.Control;
            }

            initializedOverlays[worldObject] = control;
        }

        // This method is currently used only for doors and walls to display an indicator
        public static void ClientObserving(IStaticWorldObject worldObject, bool isObserving)
        {
            if (isObserving)
            {
                var isLandClaimed = LandClaimSystem.SharedIsObjectInsideAnyArea(worldObject);
                if (isLandClaimed)
                {
                    // display only the green indicator when hover (as the red indicator is always displayed)
                    lastHoverIndicator = SetupFor(worldObject, isClaimed: true);
                }
            }
            else if (lastHoverIndicator != null)
            {
                // was hovering over that world object, destroy the indicator
                var control = (StructureLandClaimIndicator)lastHoverIndicator.Control;
                lastHoverIndicator.Destroy();
                lastHoverIndicator = null;
                control.AttachedToComponent = null;
                ControlsCache<StructureLandClaimIndicator>.Instance.Push(control);
            }
        }

        private static void DestroyControl(StructureLandClaimIndicator control)
        {
            control.AttachedToComponent?.Destroy();
            control.AttachedToComponent = null;
            ControlsCache<StructureLandClaimIndicator>.Instance.Push(control);
        }

        private static IComponentAttachedControl SetupFor(IStaticWorldObject worldObject, bool isClaimed)
        {
            var control = ControlsCache<StructureLandClaimIndicator>.Instance.Pop();
            control.Setup(isClaimed);

            var offset = worldObject.ProtoStaticWorldObject.SharedGetObjectCenterWorldOffset(worldObject);
            var component = Api.Client.UI.AttachControl(
                worldObject,
                control,
                positionOffset: offset + (0, 0.3),
                isFocusable: false);

            control.AttachedToComponent = component;
            return component;
        }

        private static void Update()
        {
            var frameNumber = Api.Client.Core.ClientFrameNumber;
            using var tempChangesList =
                Api.Shared.GetTempList<(IStaticWorldObject worldObject, StructureLandClaimIndicator control)>();

            var forceDisplayAll = ClientInputManager.IsButtonHeld(GameButton.DisplayLandClaim)
                                  || Api.Client.Input.IsKeyHeld(InputKey.Alt, evenIfHandled: true);

            var playerPosition = ClientCurrentCharacterHelper.Character?.TilePosition ?? Vector2Ushort.Zero;

            foreach (var pair in initializedOverlays)
            {
                var worldObject = pair.Key;
                if ((frameNumber + worldObject.Id) % 10 != 0)
                {
                    // don't refresh every object every frame
                    continue;
                }

                var isDisplayed = false;

                if (forceDisplayAll
                    || (worldObject.TilePosition.TileSqrDistanceTo(playerPosition)
                        <= (ClientComponentAutoDisplayStructurePointsBar.MaxDistance
                            * ClientComponentAutoDisplayStructurePointsBar.MaxDistance)))
                {
                    // can display only if close enough of if Alt/L key is held
                    isDisplayed = !LandClaimSystem.SharedIsObjectInsideAnyArea(worldObject);
                }

                var control = pair.Value;

                if (isDisplayed)
                {
                    if (control != null)
                    {
                        continue;
                    }

                    var component = SetupFor(worldObject, isClaimed: false);
                    control = (StructureLandClaimIndicator)component.Control;
                    tempChangesList.Add((worldObject, control));
                    continue;
                }

                if (control is null)
                {
                    continue;
                }

                tempChangesList.Add((worldObject, null));
                DestroyControl(control);
            }

            foreach (var entry in tempChangesList.AsList())
            {
                initializedOverlays[entry.worldObject] = entry.control;
            }
        }
    }
}