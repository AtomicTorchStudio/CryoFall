namespace AtomicTorch.CBND.CoreMod.Characters.Input.ClientPrediction
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientCurrentCharacterLagPredictionManager
    {
        /// <summary>
        /// (when player is staying)
        /// If difference between predicted client position and current client position is larger,
        /// the client will try to fix error.
        /// </summary>
        /// TODO: in the future versions we need to implement a server-side player movement to the desired staying position
        public const double MaxPredictionErrorDistanceToInterpolateWhenStaying = 0.2;

        /// <summary>
        /// (when player is moving)
        /// If difference between predicted client position and current client position is larger,
        /// the client will try to fix error.
        /// </summary>
        private const double MaxPredictionErrorDistanceToInterpolateWhenMoving = 0.025;

        /// <summary>
        /// If difference between predicted client position and current client position is larger
        /// and there are obstacles on the ray between the predicted and current positions,
        /// the client will be teleported into the predicted position.
        /// </summary>
        private const double MaxPredictionErrorDistanceToRayCollisionChecks = 0.667;

        /// <summary>
        /// If difference between predicted client position and current client position is larger,
        /// the client will be teleported into the predicted position.
        /// </summary>
        private const double MaxPredictionErrorDistanceToTeleport = 1.5;

        /// <summary>
        /// If difference between previously received server position and current server position is larger,
        /// the lag prediction system will be reset.
        /// </summary>
        private const double MaxServerDistanceSqrDifferenceToForceReset = 3 * 3;

        private const double PositionInterpolationCorrectionLerpSpeed = 10.0;

        private static readonly ICurrentGameService Game = Api.Client.CurrentGame;

        private static readonly IClientStorage SessionStorage;

        private static readonly IWorldClientService World = Api.Client.World;

        private static bool isLagPredictionEnabled;

        private static bool lastIsInterpolatingIdlePosition;

        private static Vector2D? previousServerPosition;

        private const bool IsLagPredictionCorrectionEnabled = true;

        static ClientCurrentCharacterLagPredictionManager()
        {
            SessionStorage = Api.Client.Storage.GetSessionStorage(nameof(ClientCurrentCharacterLagPredictionManager));
            if (!SessionStorage.TryLoad(out isLagPredictionEnabled))
            {
                // enabled by default
                isLagPredictionEnabled = true;
            }
        }

        public static event Action IsLagPredictionEnabledChanged;

        public static bool IsLagPredictionEnabled
        {
            get => isLagPredictionEnabled;
            set
            {
                if (isLagPredictionEnabled == value)
                {
                    return;
                }

                isLagPredictionEnabled = value;
                Api.Logger.Important("Client lag prediction is: " + (isLagPredictionEnabled ? "enabled" : "disabled"));
                SessionStorage.Save(isLagPredictionEnabled);

                if (!isLagPredictionEnabled)
                {
                    CurrentCharacterInputHistory.Instance.Clear();
                    lastIsInterpolatingIdlePosition = false;
                }

                IsLagPredictionEnabledChanged?.Invoke();
            }
        }

        public static void UpdatePosition(bool forceReset, 
                                          IDynamicWorldObject dynamicWorldObject)
        {
            var currentPlayerCharacter = ClientCurrentCharacterHelper.Character;
            if (currentPlayerCharacter is null)
            {
                return;
            }

            // we need the latest available confirmed server position
            var serverPosition = World.GetUninterpolatedPosition(
                dynamicWorldObject,
                serverFrameTime: double.MaxValue,
                out var snapshotServerTimestamp);

            if (!forceReset
                && previousServerPosition.HasValue)
            {
                var serverDistanceDeltaSqr = (serverPosition - previousServerPosition.Value).LengthSquared;
                if (serverDistanceDeltaSqr > MaxServerDistanceSqrDifferenceToForceReset)
                {
                    forceReset = true;
                    Api.Logger.Warning(
                        "Lag prediction: too big server distance difference - force reset prediction system");
                }
            }

            previousServerPosition = serverPosition;

            // current character server position updated
            // time to correct client position
            var inputHistory = CurrentCharacterInputHistory.Instance;
            if (forceReset)
            {
                previousServerPosition = null;
                inputHistory.Clear();
                inputHistory.SetLastPosition(serverPosition);
                World.SetPosition(dynamicWorldObject, serverPosition, forceReset: true);
                lastIsInterpolatingIdlePosition = false;
                return;
            }

            // TODO: actually we should do this every frame in client update method (but after simulation physics in client world)
            var clientPosition = dynamicWorldObject.Position;

            // predicted position is including the accumulated movement deltas since the snapshot timestamp
            var predictedPosition = serverPosition
                                    + inputHistory.GetClientMovementDelta(snapshotServerTimestamp);

            var visualizer = ComponentLagPredictionVisualizer.Instance;
            //visualizer.UpdateActualClientPosition(clientPosition);
            visualizer.UpdateCurrentServerPosition(serverPosition);
            visualizer.UpdateCurrentPredictedPosition(predictedPosition);

            var predictionErrorSqr = (clientPosition - predictedPosition).LengthSquared;

#pragma warning disable 162
            // ReSharper disable HeuristicUnreachableCode
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!IsLagPredictionCorrectionEnabled)
            {
                predictionErrorSqr = 0;
            }
            // ReSharper restore HeuristicUnreachableCode
#pragma warning restore 162

            if (predictionErrorSqr
                < GetPredictionErrorToleranceSqr(currentPlayerCharacter))
            {
                inputHistory.RegisterClientMovement(clientPosition);
                lastIsInterpolatingIdlePosition = false;
                return;
            }

            //Api.Logger.Error("Prediction error: " + Math.Sqrt(predictionErrorSqr).ToString("F3"));

            // too big prediction error
            Vector2D correctedClientPosition;
            var isInterpolationCorrection = dynamicWorldObject.ProtoWorldObject is PlayerCharacterSpectator
                                            || (predictionErrorSqr
                                                < MaxPredictionErrorDistanceToTeleport
                                                * MaxPredictionErrorDistanceToTeleport);
            if (isInterpolationCorrection)
            {
                var isTooBigDistanceToCheckForRayCollisions
                    = predictionErrorSqr
                      >= MaxPredictionErrorDistanceToRayCollisionChecks
                      * MaxPredictionErrorDistanceToRayCollisionChecks;

                if (isTooBigDistanceToCheckForRayCollisions
                    && GetObstaclesOnRay(dynamicWorldObject, predictedPosition, out var tempResults))
                {
                    // obstacles on the ray - cannot correct by interpolation
                    tempResults.Dispose();
                    isInterpolationCorrection = false;
                }
            }

            if (isInterpolationCorrection)
            {
                // the difference is not so big - can smoothly interpolate into predicted position
                inputHistory.RegisterClientMovement(clientPosition);
                correctedClientPosition = Vector2D.Lerp(clientPosition,
                                                        predictedPosition,
                                                        PositionInterpolationCorrectionLerpSpeed
                                                        * Api.Client.Core.DeltaTime);
                lastIsInterpolatingIdlePosition = IsIdle(currentPlayerCharacter);
            }
            else
            {
                // the difference is too big or there are obstacles - teleport client into predicted position
                correctedClientPosition = predictedPosition;
                Api.Logger.Warning(
                    "Lag prediction: too big client predicted position distance difference - force teleport client into the predicted position");
                inputHistory.Clear();
                lastIsInterpolatingIdlePosition = false;
            }

            World.SetPosition(dynamicWorldObject, correctedClientPosition, forceReset: false);
            clientPosition = dynamicWorldObject.Position;
            inputHistory.SetLastPosition(clientPosition);
        }

        private static bool GetObstaclesOnRay(
            IDynamicWorldObject worldObject,
            Vector2D predictedPosition,
            out ITempList<TestResult> tempList)
        {
            if (worldObject.ProtoWorldObject is PlayerCharacterSpectator)
            {
                tempList = null;
                return false;
            }

            var physicsSpace = World.GetPhysicsSpace();
            tempList = physicsSpace.TestLine(worldObject.Position,
                                             predictedPosition,
                                             CollisionGroup.GetDefault(),
                                             sendDebugEvent: false);
            var list = tempList.AsList();

            // remove the character itself from the test results
            for (var index = 0; index < list.Count; index++)
            {
                var body = list[index].PhysicsBody;
                if (body.AssociatedWorldObject == worldObject)
                {
                    tempList.RemoveAt(index);
                    index--;
                }
            }

            if (list.Count == 0)
            {
                // empty list - no need to return it
                tempList.Dispose();
                tempList = null;
                return false;
            }

            return true;
        }

        private static double GetPredictionErrorToleranceSqr(ICharacter character)
        {
            if (character.ProtoCharacter is PlayerCharacterSpectator)
            {
                // tolerate up to 2 tiles distance difference in spectator mode
                return 2 * 2;
            }

            if (IsIdle(character))
            {
                if (lastIsInterpolatingIdlePosition)
                {
                    // almost no tolerance when stopped moving and already sliding
                    return 0.001;
                }

                // character is not moving - prediction error tolerance is higher in that case
                // (because players are confused when their character is shifting while standing)
                return MaxPredictionErrorDistanceToInterpolateWhenStaying
                       * MaxPredictionErrorDistanceToInterpolateWhenStaying;
            }

            // character is moving 
            return MaxPredictionErrorDistanceToInterpolateWhenMoving
                   * MaxPredictionErrorDistanceToInterpolateWhenMoving;
        }

        private static bool IsIdle(ICharacter character)
        {
            var mode = PlayerCharacter.GetPublicState(character)
                                      .AppliedInput
                                      .MoveModes;

            // filter only movement direction enum flags
            mode &= CharacterMoveModes.Left
                    | CharacterMoveModes.Right
                    | CharacterMoveModes.Up
                    | CharacterMoveModes.Down;

            return mode == CharacterMoveModes.None;
        }
    }
}