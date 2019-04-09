namespace AtomicTorch.CBND.CoreMod.Systems.LandClaim
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ClientLandClaimAreaManager
    {
        private static readonly ClientLandClaimAreaRenderer RendererAreasNotOwnedByPlayer;

        private static readonly ClientLandClaimAreaRenderer RendererAreasOwnedByPlayer;

        private static readonly ClientLandClaimAreaRenderer RendererGraceAreas;

        private static readonly Color ZoneColorGraceArea = Color.FromArgb(0x30, 0x33, 0x33, 0x33);

        private static readonly Color ZoneColorNotOwnedByPlayer = Color.FromArgb(0x40, 0xDD, 0x00, 0x00);

        private static readonly Color ZoneColorOwnedByPlayer = Color.FromArgb(0x30, 0x00, 0xFF, 0x00);

        private static NetworkSyncList<ILogicObject> areaObjects;

        static ClientLandClaimAreaManager()
        {
            RendererGraceAreas = new ClientLandClaimAreaRenderer(
                ZoneColorGraceArea,
                inflateSize: LandClaimSystem.LandClaimAreaGracePaddingSize,
                drawOrder: DrawOrder.Overlay - 3);

            RendererAreasOwnedByPlayer = new ClientLandClaimAreaRenderer(
                ZoneColorOwnedByPlayer,
                drawOrder: DrawOrder.Overlay - 2);

            RendererAreasNotOwnedByPlayer = new ClientLandClaimAreaRenderer(
                ZoneColorNotOwnedByPlayer,
                drawOrder: DrawOrder.Overlay - 1);

            Api.Client.World.WorldBoundsChanged += WorldBoundsChangedHandler;
            WorldBoundsChangedHandler();

            // ReSharper disable once CanExtractXamlLocalizableStringCSharp
            ClientInputContext.Start("Land claim visualizer")
                              .HandleAll(
                                  () =>
                                  {
                                      var isDisplayed = ClientInputManager.IsButtonHeld(GameButton.DisplayLandClaim)
                                                        || Api.Client.Input.IsKeyHeld(InputKey.Alt)
                                                        || (ConstructionPlacementSystem
                                                                   .IsObjectPlacementComponentEnabled);

                                      RendererAreasOwnedByPlayer.IsVisible = isDisplayed;
                                      RendererAreasNotOwnedByPlayer.IsVisible = isDisplayed;
                                      RendererGraceAreas.IsVisible = isDisplayed;
                                  });
        }

        public static event Action<ILogicObject> AreaAdded;

        public static event Action<ILogicObject> AreaRemoved;

        private static NetworkSyncList<ILogicObject> AreaObjects
        {
            set
            {
                if (areaObjects == value)
                {
                    return;
                }

                if (areaObjects != null)
                {
                    areaObjects.ClientElementInserted -= LandClaimAreasElementInserted;
                    areaObjects.ClientElementRemoved -= LandClaimAreasElementRemoved;
                    Reset();
                }

                areaObjects = value;

                if (areaObjects != null)
                {
                    areaObjects.ClientElementInserted += LandClaimAreasElementInserted;
                    areaObjects.ClientElementRemoved += LandClaimAreasElementRemoved;
                    Rebuild();
                }
            }
        }

        public static IEnumerable<ILogicObject> EnumerateAreaObjects()
        {
            if (areaObjects != null)
            {
                return areaObjects;
            }

            return Enumerable.Empty<ILogicObject>();
        }

        public static void OnLandClaimUpgraded(ILogicObject area)
        {
            OnAreaModified(area);
        }

        public static void OnLandOwnerStateChanged(ILogicObject area, bool isOwned)
        {
            OnAreaModified(area);
        }

        public static void SetAreas(NetworkSyncList<ILogicObject> newAreas)
        {
            AreaObjects = newAreas;
        }

        private static void AddArea(ILogicObject area)
        {
            RendererGraceAreas.AddRenderer(area);
            var renderer = LandClaimSystem.ClientIsOwnedArea(area)
                               ? RendererAreasOwnedByPlayer
                               : RendererAreasNotOwnedByPlayer;

            renderer.AddRenderer(area);

            AreaAdded?.Invoke(area);
        }

        private static void LandClaimAreasElementInserted(
            NetworkSyncList<ILogicObject> source,
            int index,
            ILogicObject value)
        {
            AddArea(value);
        }

        private static void LandClaimAreasElementRemoved(
            NetworkSyncList<ILogicObject> source,
            int index,
            ILogicObject value)
        {
            RemoveArea(value);
        }

        private static void OnAreaModified(ILogicObject area)
        {
            RemoveArea(area);
            AddArea(area);
        }

        private static void Rebuild()
        {
            Reset();

            if (areaObjects == null)
            {
                return;
            }

            // add all areas to quad tree
            foreach (var area in areaObjects)
            {
                AddArea(area);
            }
        }

        private static void RemoveArea(ILogicObject area)
        {
            RendererGraceAreas.RemoveRenderer(area);
            // yes, here we're using | instead of ||
            var isRemoved = RendererAreasOwnedByPlayer.RemoveRenderer(area)
                            | RendererAreasNotOwnedByPlayer.RemoveRenderer(area);

            if (isRemoved)
            {
                AreaRemoved?.Invoke(area);
            }
        }

        private static void Reset()
        {
            if (areaObjects == null)
            {
                return;
            }

            RendererGraceAreas.Reset();
            RendererAreasOwnedByPlayer.Reset();
            RendererAreasNotOwnedByPlayer.Reset();

            foreach (var area in areaObjects)
            {
                AreaRemoved?.Invoke(area);
            }
        }

        private static void WorldBoundsChangedHandler()
        {
            Rebuild();
        }
    }
}