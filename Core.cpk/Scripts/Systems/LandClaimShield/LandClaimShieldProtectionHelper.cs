namespace AtomicTorch.CBND.CoreMod.Systems.LandClaimShield
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    internal static class LandClaimShieldProtectionHelper
    {
        /// <summary>
        /// This check prevents the abuse of the shield mechanic for this layout:
        /// * * *
        /// * X *
        /// * * *
        /// Where X is the land claim surrounded by 8 other * land claims (of max tier level),
        /// but not connected to them so normally it should provide its own shield.
        /// </summary>
        public static bool SharedIsLandClaimInsideAnotherBase(ILogicObject areasGroup)
        {
            var groupAreas = Api.IsServer
                                 ? LandClaimAreasGroup.GetPrivateState(areasGroup).ServerLandClaimsAreas
                                 : LandClaimSystem.ClientGetKnownAreasForGroup(areasGroup);

            if (groupAreas.Count() > 1)
            {
                // definitely cannot be inside another base as the base size is limited to 3*3 bases
                return false;
            }

            // Check whether this area is surrounded by other land claims.
            // We will use a flood fill approach.
            // First, create and fill an array of tiles occupied by other land claims
            // for the max land claim area bounds.
            var centerArea = groupAreas.First();
            var centerAreaState = LandClaimArea.GetPublicState(centerArea);
            var landClaimCenterTilePosition = centerAreaState.LandClaimCenterTilePosition;

            var newAreaBounds = LandClaimSystem.SharedCalculateLandClaimAreaBounds(
                landClaimCenterTilePosition,
                (ushort)(LandClaimSystem.MaxLandClaimSizeWithGraceArea.Value
                         // reduce the outer bounds as it's the buffer area
                         - LandClaimSystem.MinPaddingSizeOneDirection * 2));

            using var tempListAreas = Api.Shared.GetTempList<ILogicObject>();
            {
                using var tempList = Api.Shared.GetTempList<ILogicObject>();
                LandClaimSystem.SharedGetAreasInBounds(newAreaBounds,
                                                       tempList,
                                                       addGracePadding: true);
                foreach (var area in tempList.AsList())
                {
                    tempListAreas.AddIfNotContains(area);
                }
            }

            if (tempListAreas.Count == 1)
            {
                // there is only a single area
                return false;
            }

            var arraySizeX = newAreaBounds.Width + 2;
            var arraySizeY = newAreaBounds.Height + 2;
            var arrayOffset = new Vector2Int(newAreaBounds.X - 1, newAreaBounds.Y - 1);
            var arrayCoverage = new bool[arraySizeX, arraySizeY];

            foreach (var otherArea in tempListAreas.AsList())
            {
                if (ReferenceEquals(centerArea, otherArea))
                {
                    continue;
                }

                var bounds = LandClaimSystem.SharedGetLandClaimAreaBounds(otherArea, addGracePadding: false);
                FillArray(new RectangleInt(bounds.X - arrayOffset.X,
                                           bounds.Y - arrayOffset.Y,
                                           bounds.Width,
                                           bounds.Height));
            }

            WriteArrayToLog();

            // now we have a filled array, detect whether it represents an enclosed space by filling it from the center
            var isEnclosed = TestIsEnclosed((arraySizeX / 2, arraySizeY / 2));

            //Api.Logger.Dev("Result: " + (isEnclosed ? "Enclosed" : "Not enclosed"));
            WriteArrayToLog();

            return isEnclosed;

            void FillArray(RectangleInt bounds)
            {
                var fromX = Math.Max(0, bounds.Left);
                var fromY = Math.Max(0, bounds.Bottom);
                var toX = Math.Min(arraySizeX, bounds.Right);
                var toY = Math.Min(arraySizeY, bounds.Top);

                for (var y = fromY; y < toY; y++)
                {
                    for (var x = fromX; x < toX; x++)
                    {
                        arrayCoverage[x, y] = true;
                    }
                }
            }

            bool TestIsEnclosed(Vector2Int startPosition)
            {
                var stack = new Stack<Vector2Int>();
                stack.Push(startPosition);

                while (stack.Count > 0)
                {
                    var pos = stack.Pop();
                    if (pos.X < 0
                        || pos.Y < 0
                        || pos.X >= arraySizeX
                        || pos.Y >= arraySizeY)
                    {
                        // reached the bound - not enclosed
                        return false;
                    }

                    if (arrayCoverage[pos.X, pos.Y])
                    {
                        // another land claim there or already visited
                        continue;
                    }

                    arrayCoverage[pos.X, pos.Y] = true; // fill it
                    stack.Push((pos.X - 1, pos.Y));
                    stack.Push((pos.X + 1, pos.Y));
                    stack.Push((pos.X, pos.Y - 1));
                    stack.Push((pos.X, pos.Y + 1));
                }

                // was unable to reach the bound (filled in the enclosed space)
                return true;
            }

            void WriteArrayToLog()
            {
                // we don't need this debug data but it might be useful someday 
                return;

                var sb = new StringBuilder("Coverage array: (Y is reversed)").AppendLine();
                for (var y = 0; y < arraySizeY; y++)
                {
                    for (var x = 0; x < arraySizeX; x++)
                    {
                        sb.Append(arrayCoverage[x, y] ? "x" : ".");
                    }

                    sb.AppendLine();
                }

                Api.Logger.Dev(sb);
            }
        }
    }
}