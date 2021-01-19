namespace AtomicTorch.CBND.CoreMod.ClientComponents.FX
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentCameraScreenShakes : ClientComponent
    {
        public const float DampingSmoothTimeCameraShakes = 40;

        public const double OneShakeDuration = 2 / 60.0;

        private static ClientComponentCameraScreenShakes instance;

        private readonly List<ShakeTask> shakeTasks = new(capacity: 16);

        private Vector2F lastShakesOffset;

        public static void AddRandomShakes(float duration, float worldDistanceMin, float worldDistanceMax)
        {
            instance.AddRandomShakesInternal(duration, worldDistanceMin, worldDistanceMax);
        }

        public static void Init()
        {
            if (instance is null)
            {
                Client.Scene.CreateSceneObject(nameof(ClientComponentCameraScreenShakes))
                      .AddComponent<ClientComponentCameraScreenShakes>();
            }
        }

        public override void Update(double deltaTime)
        {
            Api.Client.Rendering.WorldCameraWorldOffset = this.ShakesGetCameraOffset(deltaTime);
        }

        protected override void OnDisable()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        protected override void OnEnable()
        {
            instance = this;
        }

        private static Vector2F ClampLength(Vector2F vector, float maxLength)
        {
            return vector.LengthSquared > maxLength * maxLength
                       ? vector.Normalized * maxLength
                       : vector;
        }

        private void AddRandomShakesInternal(float duration, float worldDistanceMin, float worldDistanceMax)
        {
            const float minDuration = 0.1f,
                        maxDuration = 1f;
            if (duration < minDuration)
            {
                // don't shake the screen if a too small explosion happens
                return;
            }

            duration = Math.Min(maxDuration, duration);

            var count = (int)(duration / OneShakeDuration);
            var availableShakesCount = this.shakeTasks.Count;
            var n = 0;
            for (var i = 0; i < count; i++)
            {
                var distance = RandomHelper.Range(worldDistanceMin, worldDistanceMax);

                var direction = RandomHelper.NextDouble() * MathConstants.DoublePI;

                var shakeA = new ShakeTask(distance, direction);
                var shakeB = new ShakeTask(distance, -direction);

                if (n >= availableShakesCount)
                {
                    // add new tasks
                    this.shakeTasks.Add(shakeA);
                    this.shakeTasks.Add(shakeB);
                    continue;
                }

                // check if we can override existing task
                if (this.shakeTasks[n].Distance >= distance)
                {
                    // the existing task is more powerful so we will not touch it
                    continue;
                }

                // update existing task
                this.shakeTasks[n++] = shakeA;

                // add or update the task after the existing task
                if (n >= availableShakesCount)
                {
                    // add new task
                    this.shakeTasks.Add(shakeB);
                }
                else
                {
                    // update existing task
                    this.shakeTasks[n++] = shakeB;
                }
            }
        }

        private Vector2F ShakesGetCameraOffset(double deltaTime)
        {
            Vector2F result;
            if (this.shakeTasks.Count == 0)
            {
                result = Vector2F.Zero;
            }
            else
            {
                var currentTask = this.shakeTasks[0];
                var currentDistance = currentTask.Distance;

                // because the game is 2D 3/4 view, we don't want to offset the camera too much in horizontal direction
                result = ((float)(0.25 * currentDistance * Math.Cos(currentTask.Direction)),
                          (float)(currentDistance * Math.Sin(currentTask.Direction)));

                currentTask.TimeLeft -= deltaTime;
                if (currentTask.TimeLeft <= 0)
                {
                    this.shakeTasks.RemoveAt(0);
                }
                else
                {
                    this.shakeTasks[0] = currentTask;
                }
            }

            // lerp the offset position to add just a little smoothness to the screen shakes
            result = ((float)MathHelper.LerpWithDeltaTime(
                          result.X,
                          this.lastShakesOffset.X,
                          deltaTime,
                          rate: DampingSmoothTimeCameraShakes),
                      (float)MathHelper.LerpWithDeltaTime(
                          result.Y,
                          this.lastShakesOffset.Y,
                          deltaTime,
                          rate: DampingSmoothTimeCameraShakes));

            this.lastShakesOffset = result;
            return result;
        }

        private struct ShakeTask
        {
            public readonly double Direction;

            public readonly double Distance;

            public double TimeLeft;

            public ShakeTask(double distance, double direction)
            {
                this.Distance = distance;
                this.Direction = direction;
                this.TimeLeft = OneShakeDuration;
            }
        }
    }
}