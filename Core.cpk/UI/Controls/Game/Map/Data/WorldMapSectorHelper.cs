namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Map.Data
{
    using System.Runtime.CompilerServices;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;
    using static WorldMapSectorProvider;

    public static class WorldMapSectorHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Ushort CalculateSectorEndPosition(Vector2Ushort position)
        {
            return (CalculateSectorStartPosition(position.X, position.Y) + (SectorWorldSize, SectorWorldSize))
                .ToVector2Ushort();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Ushort CalculateSectorStartPosition(Vector2Ushort position)
        {
            return CalculateSectorStartPosition(position.X, position.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2Ushort CalculateSectorStartPosition(ushort x, ushort y)
        {
            var worldOffset = Api.Client.World.WorldBounds.Offset;
            return ((ushort)(worldOffset.X + ((x - worldOffset.X) / SectorWorldSize) * SectorWorldSize),
                    (ushort)(worldOffset.Y + ((y - worldOffset.Y) / SectorWorldSize) * SectorWorldSize));
        }

        public static string FormatWorldPositionWithSectorCoordinate(Vector2Ushort worldPosition)
        {
            return FormatRelativePositionWithSectorCoordinate(worldPosition - Api.Client.World.WorldBounds.Offset);
        }

        private static string FormatRelativePositionWithSectorCoordinate(Vector2Ushort worldPositionRelative)
        {
            return string.Format("{0}-{1},{2}",
                                 GetSectorCoordinateTextForRelativePosition(worldPositionRelative),
                                 worldPositionRelative.X,
                                 worldPositionRelative.Y);
        }

        private static string GetSectorCoordinateTextForAbsolutePosition(Vector2Ushort worldPosition)
        {
            return GetSectorCoordinateTextForRelativePosition(worldPosition - Api.Client.World.WorldBounds.Offset);
        }

        private static string GetSectorCoordinateTextForRelativePosition(Vector2Ushort worldPositionRelative)
        {
            var offsetX = worldPositionRelative.X / SectorWorldSize;
            var offsetY = worldPositionRelative.Y / SectorWorldSize;
            var coordX = (char)('A' + offsetX);
            var coordY = 1 + offsetY;
            return coordX + coordY.ToString();
        }
    }
}