namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public static class StructureLandClaimIndicatorManager
    {
        private static readonly Dictionary<IStaticWorldObject, StructureLandClaimIndicator> InitializedOverlays
            = new(capacity: 128);

        static StructureLandClaimIndicatorManager()
        {
            ClientUpdateHelper.UpdateCallback += Update;
        }

        public static void ClientDeinitialize(IStaticWorldObject worldObject)
        {
            if (!InitializedOverlays.TryGetValue(worldObject, out var control))
            {
                return;
            }

            InitializedOverlays.Remove(worldObject);
            if (control is not null)
            {
                DestroyControl(control);
            }
        }

        public static void ClientInitialize(IStaticWorldObject worldObject)
        {
            ClientDeinitialize(worldObject); // ensure there is no existing entry for it
            InitializedOverlays[worldObject] = null;
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

            foreach (var pair in InitializedOverlays)
            {
                var worldObject = pair.Key;
                if ((frameNumber + worldObject.Id) % 10 != 0)
                {
                    // don't refresh every object every frame
                    continue;
                }

                var isDisplayed = false;

                if (forceDisplayAll
                    || ReferenceEquals(ClientComponentObjectInteractionHelper.MouseOverObject, worldObject))
                {
                    // can display only if mouse over of Alt/L key is held
                    isDisplayed = !LandClaimSystem.SharedIsObjectInsideAnyArea(worldObject);
                }

                var control = pair.Value;

                if (isDisplayed)
                {
                    if (control is not null)
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
                InitializedOverlays[entry.worldObject] = entry.control;
            }
        }
    }
}