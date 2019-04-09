namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;

    public class WallChunkDescriptionAttribute : Attribute
    {
        public readonly NeighborsPattern NeighborsPattern;

        public WallChunkDescriptionAttribute(NeighborsPattern neighborWallsPattern)
        {
            this.NeighborsPattern = neighborWallsPattern;
        }
    }
}