namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ProtoObjectLandClaimPlacementDisplayHelper
    {
        private static readonly SolidColorBrush BrushBoundBlack
            = new(Color.FromArgb(0x99, 0x00, 0x00, 0x00));

        private static readonly SolidColorBrush BrushBoundWhite
            = new(Color.FromArgb(0x99, 0xCC, 0xCC, 0xCC));

        private static readonly RenderingMaterial ClientBlueprintRestrictedTileRenderingMaterial;

        static ProtoObjectLandClaimPlacementDisplayHelper()
        {
            if (Api.IsServer)
            {
                return;
            }

            // prepare material and effect parameters
            ClientBlueprintRestrictedTileRenderingMaterial = ClientLandClaimGroupRenderer.CreateRenderingMaterial();
            ClientBlueprintRestrictedTileRenderingMaterial.EffectParameters
                                                          .Set("SpriteTexture", new TextureResource("FX/WhiteCell"))
                                                          .Set("Color",
                                                               // red color
                                                               LandClaimZoneColors.ZoneColorNotOwnedByPlayer)
                                                          .Set("IsFlipped", true);
        }

        public static void SetupBlueprint(
            Tile tile,
            IClientBlueprint blueprint,
            IProtoObjectLandClaim protoObjectLandClaim)
        {
            if (!blueprint.IsEnabled)
            {
                return;
            }

            var sceneObject = blueprint.SceneObject;
            var world = Api.Client.World;
            var rendering = Api.Client.Rendering;
            var character = ClientCurrentCharacterHelper.Character;
            var startTilePosition = LandClaimSystem.SharedCalculateLandClaimObjectCenterTilePosition(
                tile.Position,
                protoObjectLandClaim);
            var sizeWithGraceArea = protoObjectLandClaim.LandClaimWithGraceAreaSize;
            var blueprintAreaBounds = LandClaimSystem.SharedCalculateLandClaimAreaBounds(startTilePosition,
                sizeWithGraceArea);

            SetupBoundsForLandClaimsInScope(sceneObject,
                                            sceneObjectPosition: tile.Position.ToVector2D(),
                                            startTilePosition,
                                            blueprintAreaBounds,
                                            protoObjectLandClaim);

            using var tempListExceptBounds = Api.Shared.GetTempList<RectangleInt>();
            CollectLabelExclusionBoundsForBlueprint(blueprintAreaBounds, tempListExceptBounds.AsList());

            ClientLandClaimAreaManager.AddBlueprintRenderer(startTilePosition,
                                                            protoObjectLandClaim);

            // additionally highlight the restricted tiles
            var halfSize = sizeWithGraceArea / 2;
            for (var x = -halfSize; x < halfSize; x++)
            for (var y = -halfSize; y < halfSize; y++)
            {
                var checkTile = world.GetTile(startTilePosition.X + x,
                                              startTilePosition.Y + y,
                                              logOutOfBounds: false);
                ProcessFutureLandClaimAreaTile(checkTile, x, y);
            }

            AddBoundSquares(sceneObject,
                            protoObjectLandClaim,
                            positionOffset: (0, 0));
            AddBoundLabels(sceneObject,
                           sceneObjectPosition: tile.Position.ToVector2D(),
                           exceptBounds: tempListExceptBounds.AsList(),
                           protoObjectLandClaim,
                           positionOffset: (0, 0));

            void ProcessFutureLandClaimAreaTile(Tile checkTile, int offsetX, int offsetY)
            {
                if (!checkTile.IsValidTile)
                {
                    return;
                }

                var isRestrictedTile = checkTile.ProtoTile.IsRestrictingConstruction
                                       || IsRestrictedTile(checkTile);

                if (!isRestrictedTile)
                {
                    // if the tile is not restricted but any neighbour tile is restricted,
                    // restrict construction here
                    foreach (var neighborTile in checkTile.EightNeighborTiles)
                    {
                        if (IsRestrictedTile(neighborTile))
                        {
                            isRestrictedTile = true;
                            break;
                        }
                    }
                }

                if (!isRestrictedTile)
                {
                    return;
                }

                // display red tile as player cannot construct there
                var tileRenderer = rendering.CreateSpriteRenderer(
                    sceneObject,
                    ClientLandClaimGroupRenderer.TextureResourceLandClaimAreaCell,
                    DrawOrder.Overlay);

                tileRenderer.RenderingMaterial = ClientBlueprintRestrictedTileRenderingMaterial;
                tileRenderer.SortByWorldPosition = false;
                tileRenderer.Scale = 1;
                tileRenderer.PositionOffset = (offsetX + 1, offsetY + 1);
            }

            // Please note:
            // ProtoTile.IsRestrictingConstruction is not checked here
            // as it's checked separately only for the current tile.
            bool IsRestrictedTile(Tile t)
                => t.IsCliffOrSlope
                   || t.ProtoTile.Kind != TileKind.Solid
                   || !LandClaimSystem.SharedIsPositionInsideOwnedOrFreeArea(
                       t.Position,
                       character,
                       requireFactionPermission: false,
                       addGracePaddingWithoutBuffer: true);
        }

        private static void AddBoundLabels(
            IClientSceneObject sceneObject,
            Vector2D sceneObjectPosition,
            List<RectangleInt> exceptBounds,
            IProtoObjectLandClaim protoObjectLandClaim,
            Vector2D positionOffset)
        {
            var cameraFrustrum = Api.Client.Rendering.WorldCameraCurrentViewWorldBounds;
            cameraFrustrum = new BoundsDouble(minX: cameraFrustrum.MinX - 1,
                                              minY: cameraFrustrum.MinY - 1,
                                              maxX: cameraFrustrum.MaxX + 1,
                                              maxY: cameraFrustrum.MaxY + 1);

            var ui = Api.Client.UI;

            var textBlockStyle = ui.GetApplicationResource<Style>("LandClaimBoundTextBlockStyle");
            var from = protoObjectLandClaim.LandClaimSize / 2;
            var skippedTiers = 1 + (protoObjectLandClaim.LandClaimSize - LandClaimSystem.MinLandClaimSize.Value) / 2;
            var to = LandClaimSystem.MaxLandClaimSizeWithGraceArea.Value / 2;
            for (var v = from; v <= to; v++)
            {
                string text;
                if (v == to)
                {
                    text = CoreStrings.LandClaimPlacementDisplayHelper_LabelBuffer;
                }
                else
                {
                    var tier = v - from;
                    tier += skippedTiers;
                    text = string.Format(CoreStrings.LandClaimPlacementDisplayHelper_LabelTier_Format, tier);
                }

                AddLetterSet((v + 0.5, v + 0.5),   text);
                AddLetterSet((-v + 1.5, v + 0.5),  text);
                AddLetterSet((v + 0.5, -v + 1.5),  text);
                AddLetterSet((-v + 1.5, -v + 1.5), text);
            }

            void AddLetterSet(Vector2D setOffset, string text)
            {
                AddLetter(setOffset + positionOffset);
                AddLetter((setOffset.X + positionOffset.X, positionOffset.Y + 0.5));
                AddLetter((positionOffset.X + 0.5, setOffset.Y + positionOffset.Y));

                void AddLetter(Vector2D letterOffset)
                {
                    var letterWorldPosition = letterOffset + sceneObjectPosition;
                    if (!cameraFrustrum.Contains(letterWorldPosition))
                    {
                        return;
                    }

                    if (exceptBounds?.Count > 0)
                    {
                        foreach (var exceptBound in exceptBounds)
                        {
                            if (exceptBound.Contains(letterWorldPosition))
                            {
                                return;
                            }
                        }
                    }

                    ui.AttachControl(sceneObject,
                                     positionOffset: letterOffset,
                                     uiElement: new TextBlock
                                     {
                                         Text = text,
                                         Style = textBlockStyle
                                     },
                                     isFocusable: false,
                                     isScaleWithCameraZoom: false);
                }
            }
        }

        private static void AddBoundSquares(
            IClientSceneObject sceneObject,
            IProtoObjectLandClaim protoObjectLandClaim,
            Vector2D positionOffset)
        {
            positionOffset += (1, 1);

            var ui = Api.Client.UI;
            var from = protoObjectLandClaim.LandClaimSize / 2;
            var to = protoObjectLandClaim.LandClaimWithGraceAreaSize / 2;
            for (var v = from; v <= to; v++)
            {
                ui.AttachControl(
                    sceneObject,
                    positionOffset: positionOffset,
                    uiElement: new Rectangle()
                    {
                        Width = v * 2 * ScriptingConstants.TileSizeVirtualPixels,
                        Height = v * 2 * ScriptingConstants.TileSizeVirtualPixels,
                        Stroke = BrushBoundWhite,
                        StrokeThickness = 4
                    },
                    isFocusable: false,
                    isScaleWithCameraZoom: true);

                ui.AttachControl(
                    sceneObject,
                    positionOffset: positionOffset,
                    uiElement: new Rectangle()
                    {
                        Width = v * 2 * ScriptingConstants.TileSizeVirtualPixels,
                        Height = v * 2 * ScriptingConstants.TileSizeVirtualPixels,
                        Stroke = BrushBoundBlack,
                        StrokeThickness = 2
                    },
                    isFocusable: false,
                    isScaleWithCameraZoom: true);
            }
        }

        private static int CalculateIntersectionDepth(RectangleInt a, RectangleInt b)
        {
            if (!b.IntersectsLoose(a))
            {
                return -1;
            }

            var intersectionDepth = Math.Min(b.Right - a.Left,
                                             Math.Min(a.Right - b.Left,
                                                      Math.Min(b.Top - a.Bottom,
                                                               a.Top - b.Bottom)));

            if (intersectionDepth <= 0)
            {
                return -1; // no intersection
            }

            return intersectionDepth;
        }

        private static void CollectLabelExclusionBoundsForBlueprint(
            RectangleInt originBounds,
            List<RectangleInt> result)
        {
            foreach (var landClaim in
                Api.Client.World.GetStaticWorldObjectsOfProto<IProtoObjectLandClaim>())
            {
                var protoObjectLandClaim = (IProtoObjectLandClaim)landClaim.ProtoGameObject;
                var landClaimCenterPosition = LandClaimSystem
                    .SharedCalculateLandClaimObjectCenterTilePosition(
                        landClaim.TilePosition,
                        protoObjectLandClaim);

                var landClaimBounds = LandClaimSystem.SharedCalculateLandClaimAreaBounds(
                    landClaimCenterPosition,
                    protoObjectLandClaim.LandClaimWithGraceAreaSize);

                var intersectionDepth = CalculateIntersectionDepth(originBounds, landClaimBounds);
                if (intersectionDepth < 0)
                {
                    // no intersection
                    continue;
                }

                intersectionDepth = (intersectionDepth + 1) / 2;
                intersectionDepth = Math.Min(intersectionDepth,
                                             protoObjectLandClaim.LandClaimGraceAreaPaddingSizeOneDirection + 1);

                var deflated = landClaimBounds.Inflate(-intersectionDepth);
                result.Add(deflated);
            }
        }

        private static void SetupBoundsForLandClaimsInScope(
            IClientSceneObject sceneObject,
            Vector2D sceneObjectPosition,
            Vector2Ushort originTilePosition,
            RectangleInt originBounds,
            IProtoObjectLandClaim originProtoObjectLandClaim)
        {
            var landClaims = Api.Client.World.GetStaticWorldObjectsOfProto<IProtoObjectLandClaim>();
            foreach (var landClaim in landClaims)
            {
                var protoObjectLandClaim = (IProtoObjectLandClaim)landClaim.ProtoGameObject;
                var landClaimCenterPosition = LandClaimSystem
                    .SharedCalculateLandClaimObjectCenterTilePosition(
                        landClaim.TilePosition,
                        protoObjectLandClaim);

                var landClaimBounds = LandClaimSystem.SharedCalculateLandClaimAreaBounds(
                    landClaimCenterPosition,
                    protoObjectLandClaim.LandClaimWithGraceAreaSize);

                var intersectionDepth = CalculateIntersectionDepth(originBounds, landClaimBounds);
                if (intersectionDepth < 0)
                {
                    // no intersection
                    continue;
                }

                intersectionDepth = (intersectionDepth + 1) / 2;
                intersectionDepth = Math.Min(intersectionDepth,
                                             originProtoObjectLandClaim.LandClaimGraceAreaPaddingSizeOneDirection + 1);

                var exceptBounds = originBounds.Inflate(-intersectionDepth);
                using var tempList = Api.Shared.WrapObjectInTempList(exceptBounds);

                AddBoundLabels(sceneObject,
                               sceneObjectPosition,
                               exceptBounds: tempList.AsList(),
                               protoObjectLandClaim,
                               positionOffset: landClaimCenterPosition.ToVector2D()
                                               - originTilePosition.ToVector2D());
            }
        }
    }
}