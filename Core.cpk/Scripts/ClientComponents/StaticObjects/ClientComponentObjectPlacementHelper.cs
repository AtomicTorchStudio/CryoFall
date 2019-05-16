namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.UI.Services;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentObjectPlacementHelper : ClientComponent
    {
        private static ClientComponentObjectPlacementHelper instance;

        private ClientBlueprintRenderer blueprintRenderer;

        private double cachedTimeRemainsSeconds;

        private double delayRemainsSeconds;

        private ClientInputContext inputContext;

        private bool isCancelable;

        private bool isDrawConstructionGrid;

        private bool isRepeatCallbackIfHeld;

        private double maxDistanceSqr;

        private PlaceSelectedDelegate placeSelectedCallback;

        private IProtoStaticWorldObject protoStaticWorldObject;

        private IClientSceneObject sceneObjectForComponents;

        private ClientBlueprintTilesRenderer tilesBlueprint;

        private ValidateCanBuildDelegate validateCanBuildCallback;

        public delegate void PlaceSelectedDelegate(Vector2Ushort tilePosition);

        public delegate bool ValidateCanBuildDelegate(Vector2Ushort tilePosition, bool logErrors);

        public static bool HasInstance => instance != null;

        public bool IsBlockingInput { get; private set; }

        public bool IsFrozen { get; set; }

        public static void DestroyInstanceIfExist()
        {
            instance?.Destroy();
            instance = null;
        }

        public void Setup(
            IProtoStaticWorldObject protoStaticWorldObject,
            bool isCancelable,
            bool isRepeatCallbackIfHeld,
            bool isDrawConstructionGrid,
            bool isBlockingInput,
            ValidateCanBuildDelegate validateCanPlaceCallback,
            PlaceSelectedDelegate placeSelectedCallback,
            double? maxDistance = null,
            double delayRemainsSeconds = 0)
        {
            this.maxDistanceSqr = maxDistance.HasValue
                                      ? maxDistance.Value * maxDistance.Value
                                      : double.MaxValue;

            this.protoStaticWorldObject = protoStaticWorldObject;
            this.isCancelable = isCancelable;
            this.isRepeatCallbackIfHeld = isRepeatCallbackIfHeld;
            this.isDrawConstructionGrid = isDrawConstructionGrid;
            this.IsBlockingInput = isBlockingInput;
            this.placeSelectedCallback = placeSelectedCallback;
            this.validateCanBuildCallback = validateCanPlaceCallback;
            this.IsFrozen = false;

            this.delayRemainsSeconds = delayRemainsSeconds;

            this.DestroyComponents();
        }

        public void Update()
        {
            if (this.IsFrozen)
            {
                return;
            }

            if (WindowsManager.OpenedWindowsCount > 0)
            {
                // a window is opened - disable all the renderers
                this.DestroyComponents();
                return;
            }

            if (this.delayRemainsSeconds > 0)
            {
                this.delayRemainsSeconds -= Client.Core.DeltaTime;
            }

            var isUpdateRequired = false;
            if (this.blueprintRenderer == null)
            {
                // first update called
                this.SetupComponents();
                isUpdateRequired = true;
            }

            if (!isUpdateRequired
                && this.isCancelable)
            {
                if (ClientInputManager.IsButtonDown(GameButton.CancelOrClose)
                    || ClientInputManager.IsButtonDown(GameButton.ActionInteract))
                {
                    ClientInputManager.ConsumeButton(GameButton.CancelOrClose);
                    ClientInputManager.ConsumeButton(GameButton.ActionInteract);
                    // cancel building
                    this.Destroy();
                    return;
                }
            }

            var tilePosition = Client.Input.MousePointedTilePosition;
            var tilePositionVector2D = tilePosition.ToVector2D();

            this.cachedTimeRemainsSeconds -= Client.Core.DeltaTime;

            var isCanBuildThisPhase = !isUpdateRequired;

            var isPositionChanged = this.SceneObject.Position != tilePositionVector2D;
            if (isUpdateRequired
                || isPositionChanged)
            {
                this.SceneObject.Position = tilePositionVector2D;
                this.UpdateBlueprint(tilePosition);

                if (this.isRepeatCallbackIfHeld
                    && isPositionChanged
                    && this.blueprintRenderer.IsEnabled
                    && ClientInputManager.IsButtonHeld(GameButton.ActionUseCurrentItem))
                {
                    // mouse moved while LMB is held
                    this.OnPlaceSelected(tilePosition, isButtonHeld: true);
                }
            }
            else if (this.cachedTimeRemainsSeconds <= 0)
            {
                this.UpdateBlueprint(tilePosition);
            }

            if (isCanBuildThisPhase
                && this.blueprintRenderer.IsEnabled
                && ClientInputManager.IsButtonDown(GameButton.ActionUseCurrentItem))
            {
                // clicked on place
                this.OnPlaceSelected(tilePosition, isButtonHeld: false);
            }
        }

        protected override void OnDisable()
        {
            if (!this.SceneObject.IsDestroyed)
            {
                this.SceneObject.Destroy();
            }

            this.DestroyComponents();

            if (instance == this)
            {
                instance = null;
            }

            this.inputContext.Stop();
            this.inputContext = null;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            DestroyInstanceIfExist();
            instance = this;

            this.inputContext = ClientInputContext.Start(nameof(ClientComponentObjectInteractionHelper))
                                                  .HandleAll(this.Update);
        }

        private void DestroyComponents()
        {
            this.sceneObjectForComponents?.Destroy();
            this.sceneObjectForComponents = null;
            this.blueprintRenderer = null;
            this.tilesBlueprint = null;
        }

        private void OnPlaceSelected(Vector2Ushort tilePosition, bool isButtonHeld)
        {
            if (this.delayRemainsSeconds > 0)
            {
                return;
            }

            // hack to avoid excessive error messages when player hold the button and move cursor around
            var logErrors = !isButtonHeld;

            if (!this.validateCanBuildCallback(tilePosition, logErrors: logErrors))
            {
                return;
            }

            //this.blueprintRenderer.IsEnabled = false;
            //this.tilesBlueprint.IsEnabled = false;
            this.placeSelectedCallback(tilePosition);
        }

        private void SetupComponents()
        {
            this.DestroyComponents();

            this.sceneObjectForComponents = Client.Scene.CreateSceneObject(
                $"Scene object for {nameof(ClientComponentObjectPlacementHelper)} components");

            this.sceneObjectForComponents
                .AddComponent<SceneObjectPositionSynchronizer>()
                .Setup(this.SceneObject);
            this.blueprintRenderer = new ClientBlueprintRenderer(this.sceneObjectForComponents);
            this.tilesBlueprint = new ClientBlueprintTilesRenderer(this.sceneObjectForComponents);
            this.tilesBlueprint.Setup(this.protoStaticWorldObject.Layout);

            if (this.isDrawConstructionGrid)
            {
                ClientConstructionGridRendererHelper.Setup(this.sceneObjectForComponents,
                                                           this.protoStaticWorldObject);
            }
        }

        private void UpdateBlueprint(Vector2Ushort tilePosition)
        {
            if (this.blueprintRenderer == null)
            {
                return;
            }

            var tile = Client.World.GetTile(tilePosition);
            this.UpdateBlueprintCanBuild(tile);
            if (this.blueprintRenderer == null
                || !this.blueprintRenderer.IsEnabled)
            {
                return;
            }

            // setup blueprint renderer
            this.blueprintRenderer.Reset();

            this.protoStaticWorldObject.ClientSetupBlueprint(
                tile,
                this.blueprintRenderer);
        }

        private void UpdateBlueprintCanBuild(Tile tile)
        {
            foreach (var tileObj in tile.StaticObjects)
            {
                if (tileObj.ProtoStaticWorldObject == this.protoStaticWorldObject
                    || ProtoObjectConstructionSite.SharedIsConstructionOf(tileObj, this.protoStaticWorldObject))
                {
                    this.blueprintRenderer.IsEnabled = false;
                    this.tilesBlueprint.IsEnabled = false;
                    return;
                }
            }

            this.blueprintRenderer.IsEnabled = true;
            this.tilesBlueprint.IsEnabled = true;
            var tilePosition = tile.Position;

            var isCanBuild = this.validateCanBuildCallback(tilePosition, logErrors: false);
            if (this.blueprintRenderer == null)
            {
                // this component have been disabled during the validation callback
                return;
            }

            this.blueprintRenderer.IsCanBuild = isCanBuild;
            this.tilesBlueprint.IsCanBuild = isCanBuild;

            if (this.maxDistanceSqr < double.MaxValue)
            {
                var distance = tilePosition.TileSqrDistanceTo(
                    Client.Characters.CurrentPlayerCharacter.TilePosition);
                this.blueprintRenderer.IsTooFar = distance > this.maxDistanceSqr;
            }
            else
            {
                this.blueprintRenderer.IsTooFar = false;
            }

            this.blueprintRenderer.RefreshMaterial();

            // cache check result for 0.1 second
            this.cachedTimeRemainsSeconds = 0.1d;
        }
    }
}