namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientGroundExplosionAnimationHelper
    {
        public const double ExplosionGroundDuration = 1.6;

        private static readonly HashSet<Vector2Ushort> CurrentExplosions = new HashSet<Vector2Ushort>();

        public static bool HasActiveExplosion(Vector2Ushort tilePosition)
        {
            return CurrentExplosions.Contains(tilePosition);
        }

        public static bool IsGroundSpriteFlipped(Vector2Ushort tilePosition)
        {
            return PositionalRandom.Get(tilePosition,
                                        minInclusive: 0,
                                        maxExclusive: 2,
                                        seed: 218936133)
                   == 0;
        }

        public static void OnExplode(double delaySeconds, Vector2D position)
        {
            var tilePosition = position.ToVector2Ushort();
            CurrentExplosions.Add(tilePosition);

            ClientTimersSystem.AddAction(
                delaySeconds + ExplosionGroundDuration,
                () => CurrentExplosions.Remove(tilePosition));
        }

        public static void PreloadContent()
        {
            ObjectCharredGround1.ClientPreloadContent();
        }
    }
}