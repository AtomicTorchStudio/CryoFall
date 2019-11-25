namespace AtomicTorch.CBND.CoreMod.Helpers.Client.Physics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Helpers;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Helpers.Data;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;
    using AtomicTorch.GameEngine.Common.Primitives;

    [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
    public class ClientComponentPhysicsSpaceVisualizer : ClientComponent
    {
        public const int TileSize = ScriptingConstants.TileSizeVirtualPixels;

        private const double DebugShapeLifetime = 3.0d;

        // it's possible to disable tooltips to allow interactivity
        private const bool IsTooltipsEnabled = false;

        private const int StrokeThickness = 2;

        public static readonly Brush BrushClickArea = new SolidColorBrush(Color.FromArgb(0xCC, 0x00, 0xCC, 0xFF));

        public static readonly Brush BrushDynamicCollder = new SolidColorBrush(Color.FromArgb(0xCC, 0x55, 0xFF, 0x55));

        public static readonly Brush BrushHitboxMelee = new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0x00, 0x00));

        public static readonly Brush BrushHitboxRanged = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xFF, 0x00));

        public static readonly Brush BrushInteractionArea = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0x00, 0xFF));

        public static readonly Brush BrushStaticCollider = new SolidColorBrush(Color.FromArgb(0xCC, 0xFF, 0xFF, 0xFF));

        private static readonly HashSet<CollisionGroupId> EnabledLayers;

        private static readonly ISceneClientService SceneService = Client.Scene;

        private static readonly IClientStorage SessionStorage;

        private static ClientComponentPhysicsSpaceVisualizer instance;

        private static Settings settingsInstance;

        private readonly IReadOnlyList<ViewModelPhysicsGroup> legendLayers;

        private readonly IDictionary<IPhysicsBody, IComponentAttachedControl> physicsBodiesControls
            = new Dictionary<IPhysicsBody, IComponentAttachedControl>();

        private bool isClientTestRendered = true;

        private bool isServerTestRendered = true;

        private PhysicsSpaceVisualizerLegend legendControl;

        private IPhysicsSpace physicsSpace;

        private UIElementCollection visualizerControlRootChildren;

        private Canvas visualizerControlsRoot;

        static ClientComponentPhysicsSpaceVisualizer()
        {
            SessionStorage = Api.Client.Storage.GetSessionStorage(
                nameof(ClientComponentPhysicsSpaceVisualizer) + ".Settings");
            SessionStorage.RegisterType(typeof(Settings));
            SessionStorage.RegisterType(typeof(CollisionGroupId));

            if (SessionStorage.TryLoad(out settingsInstance))
            {
                EnabledLayers = settingsInstance.EnabledLayers;
                IsVisualizerEnabled = settingsInstance.IsVisualizerEnabled;
            }
            else
            {
                // set defaults
                settingsInstance.EnabledLayers = EnabledLayers = new HashSet<CollisionGroupId>()
                {
                    CollisionGroupId.Default,
                    CollisionGroupId.HitboxMelee,
                    CollisionGroupId.HitboxRanged,
                    CollisionGroupId.InteractionArea,
                    CollisionGroupId.ClickArea
                };
            }
        }

        public ClientComponentPhysicsSpaceVisualizer()
        {
            this.legendLayers = new List<ViewModelPhysicsGroup>()
            {
                new ViewModelPhysicsGroup(
                    "Static colliders",
                    CollisionGroupId.Default,
                    EnabledLayers.Contains(CollisionGroupId.Default),
                    BrushStaticCollider),
                new ViewModelPhysicsGroup(
                    "Dynamic colliders",
                    CollisionGroupId.Default,
                    EnabledLayers.Contains(CollisionGroupId.Default),
                    BrushDynamicCollder),
                new ViewModelPhysicsGroup(
                    "Hitbox melee",
                    CollisionGroupId.HitboxMelee,
                    EnabledLayers.Contains(CollisionGroupId.HitboxMelee),
                    BrushHitboxMelee),
                new ViewModelPhysicsGroup(
                    "Hitbox ranged",
                    CollisionGroupId.HitboxRanged,
                    EnabledLayers.Contains(CollisionGroupId.HitboxRanged),
                    BrushHitboxRanged),
                new ViewModelPhysicsGroup(
                    "Click area",
                    CollisionGroupId.ClickArea,
                    EnabledLayers.Contains(CollisionGroupId.ClickArea),
                    BrushClickArea),
                new ViewModelPhysicsGroup(
                    "Interaction area",
                    CollisionGroupId.InteractionArea,
                    EnabledLayers.Contains(CollisionGroupId.InteractionArea),
                    BrushInteractionArea)
            };

            foreach (var viewModelPhysicsLayer in this.legendLayers)
            {
                viewModelPhysicsLayer.IsEnabledChanged += this.ViewModelPhysicsLayerOnIsEnabledChanged;
            }
        }

        public static event Action IsInstanceExistChanged;

        public static bool IsVisualizerEnabled
        {
            get => instance != null;
            set
            {
                if (IsVisualizerEnabled == value)
                {
                    return;
                }

                settingsInstance.IsVisualizerEnabled = value;
                SessionStorage.Save(settingsInstance);

                try
                {
                    if (value)
                    {
                        // create instance
                        instance = Client.Scene.CreateSceneObject("Physics space visualizer")
                                         .AddComponent<ClientComponentPhysicsSpaceVisualizer>();
                        return;
                    }

                    // destroy instance
                    instance.SceneObject.Destroy();
                    instance = null;
                }
                finally
                {
                    IsInstanceExistChanged?.Invoke();
                }
            }
        }

        public IPhysicsSpace PhysicsSpace
        {
            get => this.physicsSpace;
            set
            {
                if (this.physicsSpace == value)
                {
                    return;
                }

                if (this.physicsSpace != null)
                {
                    this.physicsSpace.DebugPhysicsBodyAdded -= this.PhysicsBodyAddedHandler;
                    this.physicsSpace.DebugPhysicsBodyRemoved -= this.PhysicsBodyRemovedHandler;
                    this.physicsSpace.DebugShapeTesting -= this.ClientDebugShapeTestingHandler;
                }

                if (this.physicsBodiesControls.Count > 0)
                {
                    // cleanup
                    foreach (var pair in this.physicsBodiesControls)
                    {
                        DestroyPhysicsBodyVisualizer(pair.Key, pair.Value);
                    }

                    this.physicsBodiesControls.Clear();
                }

                this.physicsSpace = value;

                if (this.physicsSpace == null)
                {
                    return;
                }

                this.physicsSpace.DebugPhysicsBodyAdded += this.PhysicsBodyAddedHandler;
                this.physicsSpace.DebugPhysicsBodyRemoved += this.PhysicsBodyRemovedHandler;
                this.physicsSpace.DebugShapeTesting += this.ClientDebugShapeTestingHandler;

                foreach (var physicsBody in this.physicsSpace.PhysicsBodies)
                {
                    this.RegisterPhysicsBody(physicsBody);
                }
            }
        }

        // Do not remove this - it's for the static constructor initialization purpose only.
        public static void Init()
        {
        }

        public static void ProcessServerDebugPhysicsTesting(IPhysicsShape shape)
        {
            instance?.DrawPhysicsTest(shape, isClient: false);
        }

        public static void VisualizeTestResults(
            IList<Vector2D> testResults,
            CollisionGroup collisionGroup,
            bool isClient)
        {
            if (!IsVisualizerEnabled)
            {
                return;
            }

            foreach (var position in testResults)
            {
                instance.DrawPhysicsTest(new CircleShape(
                                             center: position,
                                             radius: 0.05,
                                             collisionGroup),
                                         isClient: isClient);
            }
        }

        protected override void OnDisable()
        {
            this.PhysicsSpace = null;

            Api.Client.UI.LayoutRootChildren.Remove(this.legendControl);
            this.legendControl.Setup(null);
            this.legendControl = null;

            Client.World.WorldBoundsChanged -= this.WorldBoundsChangedHandler;
        }

        protected override void OnEnable()
        {
            this.Refresh();

            this.legendControl = new PhysicsSpaceVisualizerLegend();
            var viewModel = new ViewModelPhysicsSpaceVisualizerLegend(
                this.legendLayers,
                this.isClientTestRendered,
                this.isServerTestRendered,
                this.ViewModelSourceFilterChangedHandler);

            this.legendControl.Setup(viewModel);

            Api.Client.UI.LayoutRootChildren.Add(this.legendControl);

            Client.World.WorldBoundsChanged += this.WorldBoundsChangedHandler;
        }

        private static void DestroyPhysicsBodyVisualizer(
            IPhysicsBody physicsBody,
            IComponentAttachedControl component)
        {
            if (physicsBody.AssociatedProtoTile != null)
            {
                component.SceneObject.Destroy();
            }
            else
            {
                component.Destroy();
            }
        }

        private void ClientDebugShapeTestingHandler(IPhysicsShape physicsShape)
        {
            this.DrawPhysicsTest(physicsShape, isClient: true);
        }

        private Shape CreateShapeControl(IPhysicsShape physicsShape)
        {
            switch (physicsShape.ShapeType)
            {
                case ShapeType.Rectangle:
                {
                    var rectangle = new Rectangle();
                    var shapeRectangle = (RectangleShape)physicsShape;
                    var size = shapeRectangle.Size * TileSize;
                    rectangle.Width = size.X;
                    rectangle.Height = size.Y;
                    var pos = shapeRectangle.Position * TileSize;
                    Canvas.SetLeft(rectangle, pos.X);
                    Canvas.SetTop(rectangle, -pos.Y - size.Y);
                    rectangle.Fill = Brushes.Transparent;
                    return rectangle;
                }

                case ShapeType.Point:
                {
                    var ellipse = new Ellipse();
                    var shapeCircle = (PointShape)physicsShape;
                    // create a small circle shape to present points
                    var radius = 0.06 * TileSize;
                    var diameter = 2 * radius;
                    ellipse.Width = diameter;
                    ellipse.Height = diameter;
                    var center = shapeCircle.Point * TileSize;
                    Canvas.SetLeft(ellipse, center.X - radius);
                    Canvas.SetTop(ellipse, -center.Y - radius);
                    BindingOperations.SetBinding(
                        ellipse,
                        Shape.FillProperty,
                        new Binding(Shape.StrokeProperty.Name)
                        {
                            Source = ellipse,
                            Mode = BindingMode.OneWay
                        });

                    return ellipse;
                }

                case ShapeType.Circle:
                {
                    var ellipse = new Ellipse();
                    var shapeCircle = (CircleShape)physicsShape;
                    var radius = shapeCircle.Radius * TileSize;
                    var diameter = 2 * radius;
                    ellipse.Width = diameter;
                    ellipse.Height = diameter;
                    var center = shapeCircle.Center * TileSize;
                    Canvas.SetLeft(ellipse, center.X - radius);
                    Canvas.SetTop(ellipse, -center.Y - radius);
                    ellipse.Fill = Brushes.Transparent;
                    return ellipse;
                }

                case ShapeType.Line:
                {
                    var line = new Line();
                    var lineShape = (LineShape)physicsShape;
                    var dir = lineShape.Direction;
                    dir = dir.Normalized;
                    // make line "infinite"
                    dir *= 50000d;
                    var pos = lineShape.BasePosition * TileSize;
                    line.X1 = pos.X - dir.X;
                    line.Y1 = -pos.Y + dir.Y;
                    line.X2 = pos.X + dir.X;
                    line.Y2 = -pos.Y - dir.Y;
                    return line;
                }

                case ShapeType.LineSegment:
                {
                    var line = new Line();
                    var lineSegmentShape = (LineSegmentShape)physicsShape;
                    line.X1 = lineSegmentShape.Point1.X * TileSize;
                    line.Y1 = -lineSegmentShape.Point1.Y * TileSize;
                    line.X2 = lineSegmentShape.Point2.X * TileSize;
                    line.Y2 = -lineSegmentShape.Point2.Y * TileSize;
                    return line;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawPhysicsTest(IPhysicsShape physicsShape, bool isClient)
        {
            if (isClient && !this.isClientTestRendered
                || !isClient && !this.isServerTestRendered)
            {
                // test rendering suppressed
                return;
            }

            if (!EnabledLayers.Contains(
                    CollisionGroups.GetCollisionGroupId(physicsShape.CollisionGroup)))
            {
                return;
            }

            var canvas = new Canvas();
            if (!this.GetBrushByCollisionGroup(physicsShape, out var brush))
            {
                brush = BrushDynamicCollder;
            }

            var shapeControl = this.CreateShapeControl(physicsShape);
            shapeControl.StrokeThickness = StrokeThickness;
            shapeControl.Stroke = brush;
            var title = (isClient ? "[CLIENT]" : "[SERVER]")
                        + " test - "
                        + physicsShape.ShapeType;

            ToolTipServiceExtend.SetToolTip(shapeControl, title);
            canvas.Children.Add(shapeControl);

            var sceneObject = Client.Scene.CreateSceneObject(title);
            Client.UI.AttachControl(
                      sceneObject,
                      canvas,
                      positionOffset: (0, 0),
                      isFocusable: IsTooltipsEnabled)
                  .UseWorldPositionZIndex = false;

            Panel.SetZIndex((UIElement)canvas.Parent, 1);

            sceneObject.Destroy(delay: DebugShapeLifetime);
        }

        private Brush GetBrush(IPhysicsBody physicsBody, IPhysicsShape shape)
        {
            if (this.GetBrushByCollisionGroup(shape, out var brush))
            {
                return brush;
            }

            if (physicsBody.IsStatic)
            {
                return BrushStaticCollider;
            }

            return BrushDynamicCollder;
        }

        private bool GetBrushByCollisionGroup(IPhysicsShape shape, out Brush brush)
        {
            if (shape.CollisionGroup == CollisionGroups.HitboxMelee)
            {
                brush = BrushHitboxMelee;
                return true;
            }

            if (shape.CollisionGroup == CollisionGroups.HitboxRanged)
            {
                brush = BrushHitboxRanged;
                return true;
            }

            if (shape.CollisionGroup == CollisionGroups.ClickArea)
            {
                brush = BrushClickArea;
                return true;
            }

            if (shape.CollisionGroup == CollisionGroups.CharacterInteractionArea)
            {
                brush = BrushInteractionArea;
                return true;
            }

            brush = null;
            return false;
        }

        private void PhysicsBodyAddedHandler(IPhysicsBody physicsBody)
        {
            this.RegisterPhysicsBody(physicsBody);
        }

        private void PhysicsBodyRemovedHandler(IPhysicsBody physicsBody)
        {
            if (!this.physicsBodiesControls.TryGetValue(physicsBody, out var component))
            {
                return;
            }

            DestroyPhysicsBodyVisualizer(physicsBody, component);
            this.physicsBodiesControls.Remove(physicsBody);
        }

        private void Refresh()
        {
            this.PhysicsSpace = null;
            this.PhysicsSpace = Client.World.GetPhysicsSpace();
        }

        private void RegisterPhysicsBody(IPhysicsBody physicsBody)
        {
            var associatedWorldObject = physicsBody.AssociatedWorldObject;
            var associatedProtoTile = physicsBody.AssociatedProtoTile;

            if (this.visualizerControlsRoot == null)
            {
                this.visualizerControlsRoot = new Canvas()
                {
                    Width = ScriptingConstants.TileSizeVirtualPixels,
                    Height = ScriptingConstants.TileSizeVirtualPixels,
                    IsHitTestVisible = IsTooltipsEnabled
                };

                this.visualizerControlRootChildren = this.visualizerControlsRoot.Children;
            }

            foreach (var shape in physicsBody.Shapes)
            {
                if (!EnabledLayers.Contains(
                        CollisionGroups.GetCollisionGroupId(shape.CollisionGroup)))
                {
                    continue;
                }

                var shapeControl = this.CreateShapeControl(shape);
                shapeControl.StrokeThickness = StrokeThickness;
                shapeControl.Stroke = this.GetBrush(physicsBody, shape);
                if (shape.CollisionGroup != CollisionGroups.CharacterInteractionArea)
                {
                    ToolTipServiceExtend.SetToolTip(
                        shapeControl,
                        associatedWorldObject != null
                            ? $"{associatedWorldObject.Name} (ID={associatedWorldObject.Id})"
                            : associatedProtoTile.ToString());
                }
                else
                {
                    // interaction area doesn't have a tooltip
                    shapeControl.IsHitTestVisible = false;
                }

                this.visualizerControlRootChildren.Add(shapeControl);
            }

            if (this.visualizerControlRootChildren.Count == 0)
            {
                // no visualizers added
                return;
            }

            IClientSceneObject sceneObject;
            if (associatedWorldObject != null)
            {
                sceneObject = associatedWorldObject.ClientSceneObject;
            }
            else
            {
                sceneObject = SceneService.CreateSceneObject(
                    "Physics visualizer for ground tile",
                    physicsBody.Position);
            }

            var component = Client.UI.AttachControl(
                sceneObject,
                this.visualizerControlsRoot,
                positionOffset: (0.5, -0.5),
                isFocusable: IsTooltipsEnabled);

            this.visualizerControlsRoot = null;
            this.visualizerControlRootChildren = null;

            this.physicsBodiesControls.Add(physicsBody, component);
        }

        private void ViewModelPhysicsLayerOnIsEnabledChanged(ViewModelPhysicsGroup viewModelPhysicsGroup)
        {
            var isEnabled = viewModelPhysicsGroup.IsEnabled;
            if (isEnabled)
            {
                if (!EnabledLayers.Add(viewModelPhysicsGroup.CollisionGroupId))
                {
                    return;
                }
            }
            else if (!EnabledLayers.Remove(viewModelPhysicsGroup.CollisionGroupId))
            {
                return;
            }

            settingsInstance.EnabledLayers = EnabledLayers;
            SessionStorage.Save(settingsInstance);

            if (viewModelPhysicsGroup.CollisionGroupId == CollisionGroupId.Default)
            {
                foreach (var entry in this.legendLayers)
                {
                    if (entry.CollisionGroupId == CollisionGroupId.Default)
                    {
                        entry.IsEnabled = isEnabled;
                    }
                }
            }

            this.Refresh();
        }

        private void ViewModelSourceFilterChangedHandler()
        {
            this.isClientTestRendered = this.legendControl.ViewModel.IsClientTestRendered;
            this.isServerTestRendered = this.legendControl.ViewModel.IsServerTestRendered;
        }

        private void WorldBoundsChangedHandler()
        {
            this.Refresh();
        }

        private struct Settings
        {
            public HashSet<CollisionGroupId> EnabledLayers;

            public bool IsVisualizerEnabled;
        }
    }
}