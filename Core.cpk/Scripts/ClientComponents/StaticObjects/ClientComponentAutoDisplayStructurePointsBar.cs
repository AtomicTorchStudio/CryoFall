namespace AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ClientComponentAutoDisplayStructurePointsBar : ClientComponent
    {
        public const double MaxDistance = 5.5;

        public const int SecondsToDisplayHealthbarAfterDamage = 30;

        private static readonly ICoreClientService Core = Client.Core;

        private static readonly IInputClientService Input = Api.Client.Input;

        private readonly IStateSubscriptionOwner subscriptionHolder = new StateSubscriptionStorage();

        private IComponentAttachedControl componentAttachedUIElement;

        private double damageThresholdFraction;

        private ObjectStructurePointsData data;

        private bool isDamaged;

        private bool isDamagedBelowThreshold;

        private float? lastStructurePoints;

        private double lastStructurePointsValueUpdateTimestamp;

        private StructurePointsBarControl structurePointsBarControl;

        public bool IsDisplayedOnlyOnMouseOver { get; set; }

        private float StructurePointsCurrent => this.data.State.StructurePointsCurrent;

        public void Setup(
            IWorldObject worldObject,
            float structurePointsMax,
            double? customDamageThresholdFraction = null)
        {
            this.Unsubscribe();
            this.data = new ObjectStructurePointsData(worldObject,
                                                      structurePointsMax);

            this.damageThresholdFraction = customDamageThresholdFraction
                                           ?? (worldObject.ProtoGameObject is IProtoObjectStructure
                                                   ? 0.98
                                                   : 1);

            this.Subscribe();
            this.Refresh();
        }

        public override void Update(double deltaTime)
        {
            if (!this.isDamaged)
            {
                return;
            }

            this.RefreshBarControl();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            this.Unsubscribe();
            this.RemoveAttachedControl();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            this.Subscribe();
            this.Refresh();
        }

        private bool CheckIsBarShouldBeVisible()
        {
            if (ClientInputManager.IsButtonHeld(GameButton.DisplayLandClaim)
                || Input.IsKeyHeld(InputKey.Alt, evenIfHandled: true)
                || (this.lastStructurePointsValueUpdateTimestamp > 0
                    && Core.ClientRealTime - this.lastStructurePointsValueUpdateTimestamp
                    < SecondsToDisplayHealthbarAfterDamage)
                || Api.IsEditor && Client.Characters.IsCurrentPlayerCharacterSpectator)
            {
                return true;
            }

            if (ClientComponentObjectInteractionHelper.MouseOverObject
                == this.data.WorldObject)
            {
                return true;
            }

            if (this.isDamagedBelowThreshold
                && !this.IsDisplayedOnlyOnMouseOver)
            {
                var playerPosition = ClientCurrentCharacterHelper.Character?.TilePosition ?? Vector2Ushort.Zero;
                var tilePosition = this.data.WorldObject.TilePosition;
                return tilePosition.TileSqrDistanceTo(playerPosition)
                       <= MaxDistance * MaxDistance;
            }

            return false;
        }

        private void Refresh()
        {
            if (!this.IsEnabled)
            {
                return;
            }

            var protoStaticWorldObject = this.data.ProtoWorldObject;
            if (protoStaticWorldObject == null)
            {
                this.RemoveAttachedControl();
                return;
            }

            var structurePointsCurrent = this.StructurePointsCurrent;

            this.isDamagedBelowThreshold = structurePointsCurrent
                                           < this.data.StructurePointsMax * this.damageThresholdFraction;

            if (this.isDamagedBelowThreshold)
            {
                this.isDamaged = true;
            }
            else
            {
                this.isDamaged = structurePointsCurrent < this.data.StructurePointsMax;
            }

            this.RefreshBarControl();
            this.lastStructurePoints = structurePointsCurrent;
        }

        private void RefreshBarControl()
        {
            if (!this.isDamaged)
            {
                this.RemoveAttachedControl();
                return;
            }

            var isBarVisible = this.CheckIsBarShouldBeVisible();
            if (!isBarVisible)
            {
                this.RemoveAttachedControl();
                return;
            }

            if (this.componentAttachedUIElement != null)
            {
                return;
            }

            var protoWorldObject = this.data.ProtoWorldObject;
            var structurePointsCurrent = this.data.State.StructurePointsCurrent;

            this.structurePointsBarControl = ControlsCache<StructurePointsBarControl>.Instance.Pop();
            this.structurePointsBarControl.Setup(this.data,
                                                 this.lastStructurePoints ?? structurePointsCurrent);

            var offset = protoWorldObject.SharedGetObjectCenterWorldOffset(
                this.SceneObject.AttachedWorldObject);

            this.componentAttachedUIElement = Api.Client.UI.AttachControl(
                this.SceneObject,
                this.structurePointsBarControl,
                positionOffset: offset + (0, 0.55),
                isFocusable: false);
        }

        private void RemoveAttachedControl()
        {
            if (this.componentAttachedUIElement == null)
            {
                return;
            }

            this.componentAttachedUIElement.Destroy();
            this.componentAttachedUIElement = null;
            ControlsCache<StructurePointsBarControl>.Instance.Push(this.structurePointsBarControl);
            this.structurePointsBarControl = null;
        }

        private void Subscribe()
        {
            if (!this.IsEnabled
                || this.data.State == null)
            {
                return;
            }

            this.data.State.ClientSubscribe(
                _ => _.StructurePointsCurrent,
                _ =>
                {
                    this.lastStructurePointsValueUpdateTimestamp = Core.ClientRealTime;
                    this.Refresh();
                },
                this.subscriptionHolder);
        }

        private void Unsubscribe()
        {
            this.subscriptionHolder.ReleaseSubscriptions();
            this.lastStructurePointsValueUpdateTimestamp = 0;
        }
    }
}