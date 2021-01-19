namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Walls
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.GameEngine.Common.Primitives;
    using static Primitives.NeighborsPattern;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public static class WallPatterns
    {
        public const double DrawOffsetNormal = 0.4;

        public const double PhysicsOffset = 0.11;

        private const double DrawOffsetDestroyed = 1;

        private const double PhysicsOffsetDestroyed = 0.27;

        private const double PhysicsOffsetDestroyedSmall = PhysicsOffsetDestroyed / 2.0;

        public static readonly WallPattern[] PatternsOverlay =
        {
            new(
                "End top",
                atlasChunk: (5, 1),
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                requiresNeighbors: Bottom),

            new(
                "End bottom",
                atlasChunk: (6, 1),
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                requiresNeighbors: Top),

            new(
                "End left",
                atlasChunk: (6, 2),
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                requiresNeighbors: Right),

            new(
                "End right",
                atlasChunk: (5, 2),
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                requiresNeighbors: Left),
        };

        public static readonly WallPattern[] PatternsPrimary =
        {
            new(
                "Cross",
                atlasChunk: (4, 0),
                requiresNeighbors: Top | Left | Right | Bottom,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                physicsNormal: _ => _.AddShapeRectangle(
                                         size: (1, 1 - 2 * PhysicsOffset),
                                         offset: (0, PhysicsOffset))
                                     .AddShapeRectangle(
                                         size: (1 - 2 * PhysicsOffset, 1),
                                         offset: (PhysicsOffset, 0))),

            new(
                "T top",
                atlasChunk: (2, 2),
                requiresNeighbors: Left | Right | Bottom,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                physicsNormal: _ => _.AddShapeRectangle(
                                         size: (1, 1 - 2 * PhysicsOffset),
                                         offset: (0, PhysicsOffset))
                                     .AddShapeRectangle(
                                         size: (1 - 2 * PhysicsOffset, 1 - PhysicsOffset),
                                         offset: (PhysicsOffset, 0)),
                physicsDestroyed: _ => _.AddShapeRectangle(
                                            size: (PhysicsOffsetDestroyed, 1 - 2 * PhysicsOffset),
                                            offset: (0, PhysicsOffset))
                                        .AddShapeRectangle(
                                            size: (1 - 2 * PhysicsOffset, PhysicsOffsetDestroyedSmall),
                                            offset: (PhysicsOffset, 0))
                                        .AddShapeRectangle(
                                            size: (PhysicsOffsetDestroyed, 1 - 2 * PhysicsOffset),
                                            offset: (1 - PhysicsOffsetDestroyed, PhysicsOffset))),

            new(
                "T bottom (inverse T)",
                atlasChunk: (3, 2),
                requiresNeighbors: Top | Left | Right,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                physicsNormal: _ => _.AddShapeRectangle(
                                         size: (1, 1 - 2 * PhysicsOffset),
                                         offset: (0, PhysicsOffset))
                                     .AddShapeRectangle(
                                         size: (1 - 2 * PhysicsOffset, 1 - PhysicsOffset),
                                         offset: (PhysicsOffset, PhysicsOffset)),
                physicsDestroyed: _ => _.AddShapeRectangle(
                                            size: (PhysicsOffsetDestroyed, 1 - 2 * PhysicsOffset),
                                            offset: (0, PhysicsOffset))
                                        .AddShapeRectangle(
                                            size: (1 - 2 * PhysicsOffset, PhysicsOffsetDestroyed),
                                            offset: (PhysicsOffset, 1 - PhysicsOffsetDestroyed))
                                        .AddShapeRectangle(
                                            size: (PhysicsOffsetDestroyed, 1 - 2 * PhysicsOffset),
                                            offset: (1 - PhysicsOffsetDestroyed, PhysicsOffset))),

            new(
                "T left",
                atlasChunk: (0, 2),
                requiresNeighbors: Top | Right | Bottom,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                physicsNormal: _ => _.AddShapeRectangle(
                                         size: (1 - PhysicsOffset, 1 - 2 * PhysicsOffset),
                                         offset: (PhysicsOffset, PhysicsOffset))
                                     .AddShapeRectangle(
                                         size: (1 - 2 * PhysicsOffset, 1),
                                         offset: (PhysicsOffset, 0)),
                physicsDestroyed: _ => _.AddShapeRectangle(
                                            size: (PhysicsOffsetDestroyed, 1 - 2 * PhysicsOffset),
                                            offset: (1 - PhysicsOffsetDestroyed, PhysicsOffset))
                                        .AddShapeRectangle(
                                            size: (1 - 2 * PhysicsOffset, PhysicsOffsetDestroyedSmall),
                                            offset: (PhysicsOffset, 0))
                                        .AddShapeRectangle(
                                            size: (1 - 2 * PhysicsOffset, PhysicsOffsetDestroyed),
                                            offset: (PhysicsOffset, 1 - PhysicsOffsetDestroyed))),

            new(
                "T right",
                atlasChunk: (1, 2),
                requiresNeighbors: Top | Left | Bottom,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                physicsNormal: _ => _.AddShapeRectangle(
                                         size: (1 - PhysicsOffset, 1 - 2 * PhysicsOffset),
                                         offset: (0, PhysicsOffset))
                                     .AddShapeRectangle(
                                         size: (1 - 2 * PhysicsOffset, 1),
                                         offset: (PhysicsOffset, 0)),
                physicsDestroyed: _ => _.AddShapeRectangle(
                                            size: (PhysicsOffsetDestroyed, 1 - 2 * PhysicsOffset),
                                            offset: (0, PhysicsOffset))
                                        .AddShapeRectangle(
                                            size: (1 - 2 * PhysicsOffset, PhysicsOffsetDestroyedSmall),
                                            offset: (PhysicsOffset, 0))
                                        .AddShapeRectangle(
                                            size: (1 - 2 * PhysicsOffset, PhysicsOffsetDestroyed),
                                            offset: (PhysicsOffset, 1 - PhysicsOffsetDestroyed))),

            new(
                "Corner top left",
                atlasChunk: (5, 0),
                requiresNeighbors: Right | Bottom,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                physicsNormal: _ => _.AddShapeRectangle(
                                   size: (1 - PhysicsOffset, 1 - PhysicsOffset),
                                   offset: (PhysicsOffset, 0)),
                physicsDestroyed: _ => _.AddShapeRectangle(
                                            size: (1 - PhysicsOffset, PhysicsOffsetDestroyedSmall),
                                            offset: (PhysicsOffset, 0))
                                        .AddShapeRectangle(
                                            size: (PhysicsOffsetDestroyed, 1 - PhysicsOffset),
                                            offset: (1 - PhysicsOffsetDestroyed, 0))),

            new(
                "Corner top right",
                atlasChunk: (6, 0),
                requiresNeighbors: Left | Bottom,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                physicsNormal: _ => _.AddShapeRectangle(
                                   size: (1 - PhysicsOffset, 1 - PhysicsOffset),
                                   offset: (0, 0)),
                physicsDestroyed: _ => _.AddShapeRectangle(
                                            size: (1 - PhysicsOffset, PhysicsOffsetDestroyedSmall),
                                            offset: (0, 0))
                                        .AddShapeRectangle(
                                            size: (PhysicsOffsetDestroyed, 1 - PhysicsOffset),
                                            offset: (0, 0))),

            new(
                "Corner bottom left",
                atlasChunk: (3, 1),
                requiresNeighbors: Top | Right,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                physicsNormal: _ => _.AddShapeRectangle(
                                   size: (1 - PhysicsOffset, 1 - PhysicsOffset),
                                   offset: (PhysicsOffset, PhysicsOffset)),
                physicsDestroyed: _ => _.AddShapeRectangle(
                                            size: (1 - PhysicsOffset, PhysicsOffsetDestroyed),
                                            offset: (PhysicsOffset, 1 - PhysicsOffsetDestroyed))
                                        .AddShapeRectangle(
                                            size: (PhysicsOffsetDestroyed, 1 - PhysicsOffset),
                                            offset: (1 - PhysicsOffsetDestroyed, PhysicsOffset))),

            new(
                "Corner bottom right",
                atlasChunk: (4, 1),
                requiresNeighbors: Top | Left,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                physicsNormal: _ => _.AddShapeRectangle(
                                   size: (1 - PhysicsOffset, 1 - PhysicsOffset),
                                   offset: (0, PhysicsOffset)),
                physicsDestroyed: _ => _.AddShapeRectangle(
                                            size: (1 - PhysicsOffset, PhysicsOffsetDestroyed),
                                            offset: (0, 1 - PhysicsOffsetDestroyed))
                                        .AddShapeRectangle(
                                            size: (PhysicsOffsetDestroyed, 1 - PhysicsOffset),
                                            offset: (0, PhysicsOffset))),

            new(
                "Vertical",
                atlasChunk: (2, 1),
                requiresNeighbors: Top | Bottom,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                physicsNormal: _ => _.AddShapeRectangle(
                                   size: (1 - 2 * PhysicsOffset, 1),
                                   offset: (PhysicsOffset, 0)),
                physicsDestroyed: _ => _.AddShapeRectangle(
                                            size: (1 - 2 * PhysicsOffset, PhysicsOffsetDestroyedSmall),
                                            offset: (PhysicsOffset, 0))
                                        .AddShapeRectangle(
                                            size: (1 - 2 * PhysicsOffset, PhysicsOffsetDestroyed),
                                            offset: (PhysicsOffset, 1 - PhysicsOffsetDestroyed))),

            new(
                "Horizontal",
                atlasChunk: (1, 0),
                requiresNeighbors: Left | Right,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetNormal,
                physicsNormal: _ => _.AddShapeRectangle(
                                   size: (1, 1 - 2 * PhysicsOffset),
                                   offset: (0, PhysicsOffset)),
                physicsDestroyed: _ => _.AddShapeRectangle(
                                            size: (PhysicsOffsetDestroyed, 1 - 2 * PhysicsOffset),
                                            offset: (1 - PhysicsOffsetDestroyed, PhysicsOffset))
                                        .AddShapeRectangle(
                                            size: (PhysicsOffsetDestroyed, 1 - 2 * PhysicsOffset),
                                            offset: (0, PhysicsOffset))),

            new(
                "End top",
                atlasChunk: Vector2Ushort.One,
                requiresNeighbors: Bottom,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                physicsNormal: _ => _.AddShapeRectangle(
                                   size: (1 - 2 * PhysicsOffset, 1 - PhysicsOffset),
                                   offset: (PhysicsOffset, 0)),
                physicsDestroyed: _ => _.AddShapeRectangle(
                                      size: (1 - 2 * PhysicsOffset, PhysicsOffsetDestroyedSmall),
                                      offset: (PhysicsOffset, 0))),

            new(
                "End bottom",
                atlasChunk: (0, 1),
                requiresNeighbors: Top,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                physicsNormal: _ => _.AddShapeRectangle(
                                   size: (1 - 2 * PhysicsOffset, 1 - PhysicsOffset),
                                   offset: (PhysicsOffset, PhysicsOffset)),
                physicsDestroyed: _ => _.AddShapeRectangle(
                                      size: (1 - 2 * PhysicsOffset, PhysicsOffsetDestroyed),
                                      offset: (PhysicsOffset, 1 - PhysicsOffsetDestroyed))),

            new(
                "End left",
                atlasChunk: (2, 0),
                requiresNeighbors: Right,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetNormal,
                physicsNormal: _ => _.AddShapeRectangle(
                                   size: (1 - PhysicsOffset, 1 - 2 * PhysicsOffset),
                                   offset: (PhysicsOffset, PhysicsOffset)),
                physicsDestroyed: _ => _.AddShapeRectangle(
                                      size: (PhysicsOffsetDestroyed, 1 - 2 * PhysicsOffset),
                                      offset: (1 - PhysicsOffsetDestroyed, PhysicsOffset))),

            new(
                "End right",
                atlasChunk: (3, 0),
                requiresNeighbors: Left,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetNormal,
                physicsNormal: _ => _.AddShapeRectangle(
                                   size: (1 - PhysicsOffset, 1 - 2 * PhysicsOffset),
                                   offset: (0, PhysicsOffset)),
                physicsDestroyed: _ => _.AddShapeRectangle(
                                      size: (PhysicsOffsetDestroyed, 1 - 2 * PhysicsOffset),
                                      offset: (0, PhysicsOffset))),

            new(
                "Standalone",
                atlasChunk: (0, 0),
                requiresNeighbors: None,
                drawOffsetNormal: DrawOffsetNormal,
                drawOffsetDestroyed: DrawOffsetDestroyed,
                physicsNormal: _ => _.AddShapeRectangle(
                                   size: (1 - 2 * PhysicsOffset, 1 - 2 * PhysicsOffset),
                                   offset: (PhysicsOffset, PhysicsOffset)),
                isValidDestroyed: false)
        };
    }
}