namespace AtomicTorch.CBND.CoreMod.Events.Base
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ServerEventLocationManager
    {
        private static readonly List<OldEventLocation> OldLocations
            = new List<OldEventLocation>();

        public static void AddUsedLocation(
            Vector2Ushort position,
            double radius,
            TimeSpan duration)
        {
            var serverTimeExpires = Api.Server.Game.FrameTime
                                    + duration.TotalSeconds;

            OldLocations.Add(
                new OldEventLocation(position,
                                     radius,
                                     serverTimeExpires));
        }

        public static void Clear()
        {
            OldLocations.Clear();
        }

        public static bool IsLocationUsedRecently(Vector2Ushort position, ushort radius)
        {
            var position2D = position.ToVector2D();
            var serverTime = Api.Server.Game.FrameTime;

            for (var index = 0; index < OldLocations.Count; index++)
            {
                var oldEvent = OldLocations[index];
                if (serverTime >= oldEvent.ServerTimeExpires)
                {
                    // the location expired and became available again
                    OldLocations.RemoveAt(index++);
                    continue;
                }

                var distance = (oldEvent.Position.ToVector2D() - position2D).Length;
                distance -= oldEvent.Radius;
                distance -= radius;
                if (distance <= 0)
                {
                    // this event was too close
                    return true;
                }
            }

            return false;
        }

        [NotPersistent]
        private readonly struct OldEventLocation
        {
            public readonly Vector2Ushort Position;

            public readonly double Radius;

            public readonly double ServerTimeExpires;

            public OldEventLocation(Vector2Ushort position, double radius, double serverTimeExpires)
            {
                this.Position = position;
                this.Radius = radius;
                this.ServerTimeExpires = serverTimeExpires;
            }
        }
    }
}