namespace AtomicTorch.CBND.CoreMod.Characters.Input.ClientPrediction
{
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class CurrentCharacterInputHistory
    {
        public static readonly CurrentCharacterInputHistory Instance = new CurrentCharacterInputHistory();

        private static readonly ICurrentGameService Game = Api.Client.CurrentGame;

        // TODO: the capacity should be adjusted to ensure it works for the current ping duration and FPS
        private readonly CycledArrayStorage<ClientPositionData> bufferDeltas
            = new CycledArrayStorage<ClientPositionData>(length: 300);

        private Vector2D? lastPosition;

        private CurrentCharacterInputHistory()
        {
        }

        public void Clear()
        {
            this.bufferDeltas.Clear();
            this.lastPosition = null;
        }

        public Vector2D GetClientMovementDelta(double fromServerTimeStamp)
        {
            var time = fromServerTimeStamp;

            var result = Vector2D.Zero;
            foreach (var entry in this.bufferDeltas.Array)
            {
                if (entry.PredictedServerTimestamp > time)
                {
                    // found not yet applied/acknowledged client movement
                    result += entry.PositionDelta;
                }
            }

            return result;
        }

        public void RegisterClientMovement(Vector2D position)
        {
            Vector2D positionDelta;
            if (this.lastPosition.HasValue)
            {
                positionDelta = position - this.lastPosition.Value;
                if (positionDelta.LengthSquared <= double.Epsilon)
                {
                    // not moved
                    return;
                }
            }
            else
            {
                positionDelta = Vector2D.Zero;
            }

            this.lastPosition = position;

            // calculate half of RTT
            var halfRtt = Game.PingGameSeconds / 2.0;

            // calculate the current server time
            // (please note that "server frame time" is reduced by interp and half RTT due to interpolation and latency)
            var predictedServerTimeStamp = Game.ServerFrameTimeApproximated
                                           + Game.PhysicsInterpolationInterval
                                           + halfRtt;

            // current client simulation timestamp is ahead of the server's processing timestamp
            // how much ahead? by half RTT of course
            predictedServerTimeStamp += halfRtt;

            this.bufferDeltas.Add(
                new ClientPositionData(
                    positionDelta,
                    predictedServerTimeStamp));
        }

        public void SetLastPosition(Vector2D currentClientPosition)
        {
            this.lastPosition = currentClientPosition;
        }

        [NotPersistent]
        private readonly struct ClientPositionData
        {
            public readonly Vector2D PositionDelta;

            public readonly double PredictedServerTimestamp;

            public ClientPositionData(
                Vector2D positionDelta,
                double predictedServerTimestamp)
            {
                this.PositionDelta = positionDelta;
                this.PredictedServerTimestamp = predictedServerTimestamp;
            }
        }
    }
}