namespace AtomicTorch.CBND.CoreMod.Characters.Input.ClientPrediction
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ComponentLagPredictionVisualizer : ClientComponent
    {
        private static readonly IClientStorage SessionStorage;

        private static ComponentLagPredictionVisualizer instance;

        private static bool isVisualizerEnabled;

        private IClientSceneObject sceneObjectClientActual;

        private IClientSceneObject sceneObjectClientPredicted;

        private IClientSceneObject sceneObjectServer;

        static ComponentLagPredictionVisualizer()
        {
            SessionStorage = Api.Client.Storage.GetSessionStorage(
                $"{nameof(ComponentLagPredictionVisualizer)}.{nameof(IsVisualizerEnabled)}");
            SessionStorage.TryLoad(out isVisualizerEnabled);

            ClientCurrentCharacterLagPredictionManager.IsLagPredictionEnabledChanged
                += () =>
                   {
                       if (!ClientCurrentCharacterLagPredictionManager.IsLagPredictionEnabled)
                       {
                           DestroyInstance();
                       }
                   };
        }

        public static ComponentLagPredictionVisualizer Instance
        {
            get
            {
                if (instance is null)
                {
                    instance = Api.Client.Scene.CreateSceneObject(nameof(ComponentLagPredictionVisualizer))
                                  .AddComponent<ComponentLagPredictionVisualizer>(
                                      isEnabled: isVisualizerEnabled);
                }

                return instance;
            }
        }

        public static bool IsVisualizerEnabled
        {
            get => isVisualizerEnabled;
            set
            {
                if (isVisualizerEnabled == value)
                {
                    return;
                }

                isVisualizerEnabled = value;
                SessionStorage.Save(isVisualizerEnabled);

                if (instance is not null)
                {
                    instance.IsEnabled = isVisualizerEnabled;
                }
            }
        }

        public override void Update(double deltaTime)
        {
            base.Update(deltaTime);
            var character = ClientCurrentCharacterHelper.Character;
            if (character is not null)
            {
                this.UpdateActualClientPosition(character.Position);
            }
        }

        public void UpdateCurrentPredictedPosition(Vector2D predictedPosition)
        {
            if (this.IsEnabled)
            {
                this.sceneObjectClientPredicted.Position = predictedPosition;
            }
        }

        public void UpdateCurrentServerPosition(Vector2D serverPosition)
        {
            if (this.IsEnabled)
            {
                this.sceneObjectServer.Position = serverPosition;
            }
        }

        protected override void OnDisable()
        {
            this.sceneObjectServer.Destroy();
            this.sceneObjectClientPredicted.Destroy();
            this.sceneObjectClientActual.Destroy();

            this.sceneObjectServer = null;
            this.sceneObjectClientPredicted = null;
            this.sceneObjectClientActual = null;
        }

        protected override void OnEnable()
        {
            if (!isVisualizerEnabled)
            {
                // should be impossible
                throw new Exception("This is a disabled component!");
            }

            this.sceneObjectServer = Client.Scene.CreateSceneObject(this + " - Server ghost");
            this.sceneObjectClientPredicted = Client.Scene.CreateSceneObject(this + " - Client prediction");
            this.sceneObjectClientActual = Client.Scene.CreateSceneObject(this + " - Client current");

            CreateVisualizer(this.sceneObjectServer,          Colors.Aqua);
            CreateVisualizer(this.sceneObjectClientPredicted, Colors.Red);
            CreateVisualizer(this.sceneObjectClientActual,    Colors.GreenYellow);
        }

        private static void CreateVisualizer(IClientSceneObject sceneObject, Color color)
        {
            // create circle - UI ellipse control
            var circle = new Ellipse();

            var radius = 25;
            var diameter = 2 * radius;
            circle.Width = diameter;
            circle.Height = diameter;

            Canvas.SetLeft(circle, -radius);
            Canvas.SetTop(circle, -radius);

            circle.Fill = new SolidColorBrush(color)
            {
                Opacity = 0.6
            };

            // attach circle to scene object (in canvas)
            var canvas = new Canvas();
            canvas.Children.Add(circle);
            Client.UI.AttachControl(
                      sceneObject,
                      canvas,
                      positionOffset: (0, 0),
                      isFocusable: true)
                  .SetCustomZIndex(1);
        }

        private static void DestroyInstance()
        {
            if (instance is null)
            {
                return;
            }

            instance.SceneObject.Destroy();
            instance = null;
        }

        private void UpdateActualClientPosition(Vector2D clientPosition)
        {
            if (this.IsEnabled)
            {
                this.sceneObjectClientActual.Position = clientPosition;
            }
        }
    }
}