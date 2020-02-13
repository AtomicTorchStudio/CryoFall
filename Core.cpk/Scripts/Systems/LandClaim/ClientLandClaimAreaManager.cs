namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public static class ClientLandClaimAreaManager
    {
        private static readonly ClientLandClaimGroupsRendererManager RendererManagerGraceAreas;

        private static readonly ClientLandClaimGroupsRendererManager RendererManagerNotOwnedByPlayer;

        private static readonly ClientLandClaimGroupsRendererManager RendererManagerOwnedByPlayer;

        private static readonly Dictionary<ILogicObject, StateSubscriptionStorage> StateSubscriptionStorages
            = new Dictionary<ILogicObject, StateSubscriptionStorage>();

        static ClientLandClaimAreaManager()
        {
            RendererManagerGraceAreas = new ClientLandClaimGroupsRendererManager(
                LandClaimZoneColors.ZoneColorGraceArea,
                drawOrder: DrawOrder.Overlay - 3,
                isGraceAreaRenderer: true,
                isFlippedTexture: true);

            RendererManagerNotOwnedByPlayer = new ClientLandClaimGroupsRendererManager(
                LandClaimZoneColors.ZoneColorNotOwnedByPlayer,
                drawOrder: DrawOrder.Overlay - 2,
                isFlippedTexture: true);

            RendererManagerOwnedByPlayer = new ClientLandClaimGroupsRendererManager(
                LandClaimZoneColors.ZoneColorOwnedByPlayer,
                drawOrder: DrawOrder.Overlay - 1,
                isFlippedTexture: false);

            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            ClientInputContext.Start("Land claim visualizer")
                              .HandleAll(
                                  () =>
                                  {
                                      var isDisplayed = ClientInputManager.IsButtonHeld(GameButton.DisplayLandClaim)
                                                        || Api.Client.Input.IsKeyHeld(InputKey.Alt,
                                                                                      evenIfHandled: true)
                                                        || ConstructionPlacementSystem
                                                            .IsObjectPlacementComponentEnabled;

                                      if (isDisplayed)
                                      {
                                          var isDisplayOverlays = Api.Client.Input.IsKeyHeld(InputKey.Control,
                                                                                             evenIfHandled: true);

                                          RendererManagerOwnedByPlayer.IsDisplayOverlays
                                              = RendererManagerNotOwnedByPlayer.IsDisplayOverlays
                                                    = RendererManagerGraceAreas.IsDisplayOverlays
                                                          = isDisplayOverlays;
                                      }

                                      RendererManagerOwnedByPlayer.IsEnabled
                                          = RendererManagerNotOwnedByPlayer.IsEnabled
                                                = RendererManagerGraceAreas.IsEnabled
                                                      = isDisplayed;
                                  });
        }

        public static event Action<ILogicObject> AreaAdded;

        public static event Action<ILogicObject> AreaRemoved;

        public static IEnumerable<ILogicObject> EnumerateAreaObjects()
        {
            return LandClaimSystem.SharedEnumerateAllAreas();
        }

        public static void OnLandClaimUpgraded(ILogicObject area)
        {
            OnAreaModified(area);
        }

        public static void OnLandOwnerStateChanged(ILogicObject area, bool isOwned)
        {
            OnAreaModified(area);
        }

        /// <summary>
        /// This method should be called only by LandClaimSystem.
        /// </summary>
        internal static void AddArea(ILogicObject area)
        {
            if (StateSubscriptionStorages.ContainsKey(area))
            {
                return;
            }

            // register for group change event
            var stateSubscriptionStorage = new StateSubscriptionStorage();
            StateSubscriptionStorages[area] = stateSubscriptionStorage;

            var areaPublicState = LandClaimArea.GetPublicState(area);
            areaPublicState.ClientSubscribe(
                o => o.LandClaimAreasGroup,
                newValue =>
                {
                    //Api.Logger.Dev($"Received LandClaimAreasGroup changed: {newValue} for {area}");
                    OnAreaModified(area);
                },
                stateSubscriptionStorage);

            // register area
            RendererManagerGraceAreas.RegisterArea(area);

            var renderer = LandClaimSystem.ClientIsOwnedArea(area)
                               ? RendererManagerOwnedByPlayer
                               : RendererManagerNotOwnedByPlayer;

            renderer.RegisterArea(area);

            AreaAdded?.Invoke(area);
        }

        /// <summary>
        /// This method should be called only by LandClaimSystem.
        /// </summary>
        internal static void RemoveArea(ILogicObject area)
        {
            if (!StateSubscriptionStorages.TryGetValue(area, out var token))
            {
                return;
            }

            token.Dispose();
            StateSubscriptionStorages.Remove(area);

            RendererManagerGraceAreas.UnregisterArea(area);
            RendererManagerOwnedByPlayer.UnregisterArea(area);
            RendererManagerNotOwnedByPlayer.UnregisterArea(area);

            AreaRemoved?.Invoke(area);
        }

        private static void OnAreaModified(ILogicObject area)
        {
            if (!area.IsInitialized)
            {
                return;
            }

            RemoveArea(area);
            AddArea(area);
        }
    }
}