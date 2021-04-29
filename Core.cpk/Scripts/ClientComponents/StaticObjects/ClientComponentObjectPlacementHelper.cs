namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.ConstructionSite;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Services;
    using AtomicTorch.CBND.GameApi.Data.World;
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

        private bool isCanBuildChanged;

        private bool isCancelable;

        private bool isDrawConstructionGrid;

        private bool isRepeatCallbackIfHeld;

        private PlaceSelectedDelegate placeSelectedCallback;

        private IProtoStaticWorldObject protoStaticWorldObject;

        private IClientSceneObject sceneObjectForComponents;

        private ClientBlueprintTilesRenderer tilesBlueprint;

        private ValidateCanBuildDelegate validateCanBuildCallback;

        public delegate void PlaceSelectedDelegate(Vector2Ushort tilePosition);

        public delegate void ValidateCanBuildDelegate(
            Vector2Ushort tilePosition,
            bool logErrors,
            out string errorMessage,
            out bool canPlace,
            out bool isTooFar);

        public static bool HasInstance => instance is not null;

        public bool HideBlueprintOnOverlapWithTheSameObject { get; set; } = true;

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
            double delayRemainsSeconds = 0)
        {
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
            if (this.IsFrozen
                || this.IsDestroyed)
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
            this.isCanBuildChanged = false;
            if (this.blueprintRenderer is null)
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
                    // cancel/quit placement mode by Escape key or interaction (RMB click)
                    ClientInputManager.ConsumeButton(GameButton.CancelOrClose);
                    ClientInputManager.ConsumeButton(GameButton.ActionInteract);
                    this.Destroy();
                    return;
                }
            }

            var tilePosition = (Client.Input.MousePointedTilePosition
                                + this.protoStaticWorldObject.BlueprintTileOffset)
                .ToVector2Ushort();

            var tilePosition2D = tilePosition.ToVector2D();

            this.cachedTimeRemainsSeconds -= Client.Core.DeltaTime;

            var isCanBuildThisPhase = !isUpdateRequired;

            var isPositionChanged = this.SceneObject.Position != tilePosition2D;
            if (isUpdateRequired
                || isPositionChanged
                || this.cachedTimeRemainsSeconds <= 0)
            {
                this.SceneObject.Position = tilePosition2D;
                this.UpdateBlueprint(tilePosition);

                if (this.isRepeatCallbackIfHeld
                    && (isPositionChanged
                        || this.isCanBuildChanged)
                    && (this.blueprintRenderer?.IsEnabled ?? false)
                    && ClientInputManager.IsButtonHeld(GameButton.ActionUseCurrentItem))
                {
                    // mouse moved while LMB is held
                    this.OnPlaceSelected(tilePosition, isButtonHeld: true);
                }
            }

            if (isCanBuildThisPhase
                && (this.blueprintRenderer?.IsEnabled ?? false)
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

            this.inputContext = ClientInputContext.Start(nameof(ClientComponentObjectPlacementHelper))
                                                  .HandleAll(this.Update);
        }

        private void DestroyComponents()
        {
            this.sceneObjectForComponents?.Destroy();
            this.sceneObjectForComponents = null;
            this.blueprintRenderer = null;
            this.tilesBlueprint = null;

            ClientLandClaimAreaManager.RemoveBlueprintRenderer();
        }

        private void OnPlaceSelected(Vector2Ushort tilePosition, bool isButtonHeld)
        {
            if (this.delayRemainsSeconds > 0)
            {
                return;
            }

            this.validateCanBuildCallback(tilePosition,
                                          logErrors: !isButtonHeld,
                                          out _,
                                          out var canPlace,
                                          out var isTooFar);

            if (!canPlace || isTooFar)
            {
                return;
            }

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

            this.blueprintRenderer = new ClientBlueprintRenderer(
                this.sceneObjectForComponents,
                isConstructionSite: false,
                this.protoStaticWorldObject.SharedGetObjectCenterWorldOffset(null));

            this.blueprintRenderer.IsEnabled = false;
            this.tilesBlueprint = new ClientBlueprintTilesRenderer(this.sceneObjectForComponents);
            this.tilesBlueprint.Setup(this.protoStaticWorldObject.Layout);
            this.tilesBlueprint.IsEnabled = false;

            if (this.isDrawConstructionGrid)
            {
                ClientConstructionGridRendererHelper.Setup(this.sceneObjectForComponents,
                                                           this.protoStaticWorldObject);
            }
        }

        private void UpdateBlueprint(Vector2Ushort tilePosition)
        {
            if (this.blueprintRenderer is null)
            {
                return;
            }

            var tile = Client.World.GetTile(tilePosition);
            this.UpdateBlueprintCanBuild(tile);
            if (this.blueprintRenderer is null
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
            if (this.HideBlueprintOnOverlapWithTheSameObject)
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
            }

            this.blueprintRenderer.IsEnabled = true;
            this.tilesBlueprint.IsEnabled = true;
            var tilePosition = tile.Position;
            this.validateCanBuildCallback(tilePosition,
                                          logErrors: false,
                                          out var errorMessage,
                                          out var canPlace,
                                          out var isTooFar);

            if (this.blueprintRenderer is null)
            {
                // this component have been disabled during the validation callback
                return;
            }

            if (this.blueprintRenderer.IsCanBuild != canPlace
                || this.blueprintRenderer.IsTooFar != isTooFar)
            {
                this.isCanBuildChanged = true;
            }

            this.blueprintRenderer.IsCanBuild = canPlace;
            this.tilesBlueprint.IsCanBuild = canPlace;
            this.blueprintRenderer.IsTooFar = isTooFar;

            if (isTooFar && string.IsNullOrEmpty(errorMessage))
            {
                errorMessage = CoreStrings.Notification_TooFar;
            }

            this.blueprintRenderer.CannotBuildReason = errorMessage;
            this.blueprintRenderer.RefreshMaterial();

            // cache check result for 0.1 second
            this.cachedTimeRemainsSeconds = 0.1;
        }
    }
}