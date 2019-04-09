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
            var time = fromServerTimeStamp - Game.PingGameSeconds / 2;

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
            var positionDelta = this.lastPosition != null
                                    ? position - this.lastPosition.Value
                                    : Vector2D.Zero;

            if (positionDelta.LengthSquared <= double.Epsilon)
            {
                // not moved
                return;
            }

            this.lastPosition = position;

            var serverTimeStamp = CalculatePredictedServerTimestamp();

            this.bufferDeltas.Add(
                new ClientPositionData(
                    positionDelta,
                    serverTimeStamp));
        }

        public void SetLastPosition(Vector2D currentClientPosition)
        {
            this.lastPosition = currentClientPosition;
        }

        private static double CalculatePredictedServerTimestamp()
        {
            return Game.ServerFrameTimeRounded
                   + Game.PhysicsInterpolationInterval
                   + Game.PingGameSeconds / 2;
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