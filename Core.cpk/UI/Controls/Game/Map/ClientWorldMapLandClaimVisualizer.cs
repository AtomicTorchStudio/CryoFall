namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaimShield;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientWorldMapLandClaimVisualizer : BaseWorldMapVisualizer
    {
        private readonly ClientWorldMapLandClaimsGroupVisualizer landClaimGroupVisualizer;

        private readonly Dictionary<ILogicObject, LandClaimMapData> visualizedAreas
            = new();

        public ClientWorldMapLandClaimVisualizer(
            WorldMapController worldMapController,
            ClientWorldMapLandClaimsGroupVisualizer landClaimGroupVisualizer)
            : base(worldMapController)
        {
            this.landClaimGroupVisualizer = landClaimGroupVisualizer;

            ClientLandClaimAreaManager.AreaAdded += this.AreaAddedHandler;
            ClientLandClaimAreaManager.AreaRemoved += this.AreaRemovedHandler;

            foreach (var area in ClientLandClaimAreaManager.EnumerateAreaObjects())
            {
                this.AreaAddedHandler(area);
            }
        }

        protected override void DisposeInternal()
        {
            ClientLandClaimAreaManager.AreaAdded -= this.AreaAddedHandler;
            ClientLandClaimAreaManager.AreaRemoved -= this.AreaRemovedHandler;

            if (this.visualizedAreas.Count > 0)
            {
                foreach (var visualizedArea in this.visualizedAreas.ToList())
                {
                    this.AreaRemovedHandler(visualizedArea.Key);
                }
            }
        }

        private void AreaAddedHandler(ILogicObject area)
        {
            if (!LandClaimSystem.ClientIsOwnedArea(area, requireFactionPermission: false))
            {
                return;
            }

            if (this.visualizedAreas.ContainsKey(area))
            {
                Api.Logger.Error("Land claim area already has the map visualizer: " + area);
                return;
            }

            var isFounder = string.Equals(LandClaimArea.GetPrivateState(area).LandClaimFounder,
                                          ClientCurrentCharacterHelper.Character.Name,
                                          StringComparison.Ordinal);

            var areaClanTag = LandClaimSystem.SharedGetAreaOwnerFactionClanTag(area);
            var isFactionOwned = !string.IsNullOrEmpty(areaClanTag);
            // it's not necessary to check as client can see on the map only the owned land claims
            // however, the reason we've disabled this check is due to the timing issue
            // when player joins the faction its clan tag may arrive after the land claims
            //&& FactionSystem.ClientCurrentFactionClanTag == areaClanTag;

            this.visualizedAreas[area] = new LandClaimMapData(area,
                                                              this,
                                                              this.landClaimGroupVisualizer,
                                                              isFounder,
                                                              isFactionOwned);
        }

        private void AreaRemovedHandler(ILogicObject area)
        {
            if (!this.visualizedAreas.TryGetValue(area, out var data))
            {
                return;
            }

            this.visualizedAreas.Remove(area);
            data.Dispose();
        }

        public class LandClaimMapData : IDisposable
        {
            private readonly ILogicObject area;

            private readonly ClientWorldMapLandClaimsGroupVisualizer landClaimGroupVisualizer;

            private readonly ClientWorldMapLandClaimVisualizer visualizer;

            private WorldMapMarkLandClaim markControl;

            public LandClaimMapData(
                ILogicObject area,
                ClientWorldMapLandClaimVisualizer visualizer,
                ClientWorldMapLandClaimsGroupVisualizer landClaimGroupVisualizer,
                bool isFounder,
                bool isFactionOwned)
            {
                this.area = area;

                this.visualizer = visualizer;
                this.landClaimGroupVisualizer = landClaimGroupVisualizer;

                // add land claim mark control to map
                this.markControl = new WorldMapMarkLandClaim()
                {
                    IsFounder = isFounder,
                    IsFactionOwned = isFactionOwned,
                    Area = area
                };

                var canvasPosition = this.GetAreaCanvasPosition();
                Canvas.SetLeft(this.markControl, canvasPosition.X);
                Canvas.SetTop(this.markControl, canvasPosition.Y);
                Panel.SetZIndex(this.markControl, 12);

                visualizer.AddControl(this.markControl);
                this.landClaimGroupVisualizer.Register(this.area);

                ClientUpdateHelper.UpdateCallback += this.UpdateCallback;
            }

            public void Dispose()
            {
                if (this.markControl is not null)
                {
                    this.visualizer.RemoveControl(this.markControl);
                    this.markControl = null;
                }

                this.landClaimGroupVisualizer.Unregister(this.area);

                ClientUpdateHelper.UpdateCallback -= this.UpdateCallback;
            }

            private Vector2D GetAreaCanvasPosition()
            {
                return this.visualizer.WorldToCanvasPosition(this.GetAreaWorldPosition());
            }

            private Vector2D GetAreaWorldPosition()
            {
                var bounds = LandClaimSystem.SharedGetLandClaimAreaBounds(this.area);
                return (bounds.X + bounds.Width / 2.0,
                        bounds.Y + bounds.Height / 2.0);
            }

            private void UpdateCallback()
            {
                var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(this.area);
                var status = LandClaimShieldProtectionSystem.SharedGetShieldPublicStatus(areasGroup);

                switch (status)
                {
                    case ShieldProtectionStatus.Active:
                        this.markControl.IsUnderShield = true;
                        break;

                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    case ShieldProtectionStatus.Activating:
                        // flicker the icon for half a second
                        this.markControl.IsUnderShield = Math.Round(Api.Client.Core.ClientRealTime % 1.0)
                                                         == 0.0;
                        break;

                    default:
                        this.markControl.IsUnderShield = false;
                        break;
                }
            }
        }
    }
}