namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Crates
{
    using System;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectAccessMode;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectCrate
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectCrate,
          IProtoObjectWithOwnersList,
          IProtoObjectWithAccessMode
        where TPrivateState : ObjectCratePrivateState, new()
        where TPublicState : ObjectCratePublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        // how long the items dropped on the ground from the destroyed crate should remain there
        private static readonly TimeSpan DestroyedCrateDroppedItemsDestructionTimeout = TimeSpan.FromDays(1);

        public abstract bool HasOwnersList { get; }

        public override string InteractionTooltipText => InteractionTooltipTexts.Open;

        public bool IsAutoEnterPrivateScopeOnInteraction => true;

        public bool IsClosedAccessModeAvailable => false;

        public override bool IsRelocatable => true;

        public virtual bool IsSupportItemIcon => true;

        public abstract byte ItemsSlotsCount { get; }

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        public override float StructurePointsMaxForConstructionSite
            => this.StructurePointsMax / 25;

        protected virtual Vector2D ItemIconOffset => (0, 0.0425);

        protected virtual double ItemIconScale => 0.3;

        protected virtual IProtoItemsContainer ItemsContainerType
            => Api.GetProtoEntity<ItemsContainerDefault>();

        protected virtual ITextureResource TextureResourceIconPlate { get; }
            = new TextureResource("StaticObjects/Structures/Crates/ObjectCrate_Plate");

        public void ClientSetIconSource(IStaticWorldObject worldObjectCrate, IProtoEntity iconSource)
        {
            if (GetPublicState(worldObjectCrate).IconSource == iconSource)
            {
                return;
            }

            this.CallServer(_ => _.ServerRemote_SetIconSource(worldObjectCrate, iconSource));
        }

        public override void ServerOnBuilt(IStaticWorldObject structure, ICharacter byCharacter)
        {
            WorldObjectOwnersSystem.ServerOnBuilt(structure, byCharacter);
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);

            ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                gameObject.OccupiedTile,
                GetPrivateState(gameObject).ItemsContainer,
                destroyTimeout: DestroyedCrateDroppedItemsDestructionTimeout.TotalSeconds);
        }

        public bool SharedCanEditOwners(IWorldObject worldObject, ICharacter byOwner)
        {
            return this.HasOwnersList;
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            return base.SharedCanInteract(character, worldObject, writeToLog)
                   && (CreativeModeSystem.SharedIsInCreativeMode(character)
                       || WorldObjectAccessModeSystem.SharedHasAccess(character, worldObject, writeToLog));
        }

        BaseUserControlWithWindow IInteractableProtoWorldObject.ClientOpenUI(IWorldObject worldObject)
        {
            var staticWorldObject = (IStaticWorldObject)worldObject;
            var privateState = GetPrivateState(staticWorldObject);
            return this.ClientOpenUI(staticWorldObject, privateState);
        }

        void IInteractableProtoWorldObject.ServerOnClientInteract(ICharacter who, IWorldObject worldObject)
        {
        }

        void IInteractableProtoWorldObject.ServerOnMenuClosed(ICharacter who, IWorldObject worldObject)
        {
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            this.ClientSetupIconRenderer(data);
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            InteractableWorldObjectHelper.ClientStartInteract(data.GameObject);
        }

        protected virtual BaseUserControlWithWindow ClientOpenUI(
            IStaticWorldObject worldObject,
            TPrivateState privateState)
        {
            return WindowCrateContainer.Show(worldObject, privateState);
        }

        protected virtual void ClientSetupIconRenderer(ClientInitializeData data)
        {
            if (!this.IsSupportItemIcon)
            {
                return;
            }

            var clientState = data.ClientState;

            var spriteRenderIconPlate = Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                this.TextureResourceIconPlate,
                spritePivotPoint: (0.5, 0.5));

            var spriteRenderIcon = Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                TextureResource.NoTexture,
                spritePivotPoint: (0.5, 0.5));

            spriteRenderIconPlate.Scale = 1.2;
            spriteRenderIcon.Scale = 1.95 * this.ItemIconScale;

            this.ClientSetupIconSpriteRenderer(clientState.Renderer, spriteRenderIconPlate);
            this.ClientSetupIconSpriteRenderer(clientState.Renderer, spriteRenderIcon);

            var publicState = data.PublicState;
            publicState.ClientSubscribe(
                _ => _.IconSource,
                _ => UpdateIcon(),
                clientState);

            UpdateIcon();

            // local method for updating public liquid state
            void UpdateIcon()
            {
                var iconSource = publicState.IconSource;
                spriteRenderIcon.IsEnabled = spriteRenderIconPlate.IsEnabled = iconSource is not null;
                spriteRenderIcon.TextureResource = ClientCrateIconHelper.GetIcon(iconSource);
            }
        }

        protected virtual void ClientSetupIconSpriteRenderer(
            IComponentSpriteRenderer spriteRenderer,
            IComponentSpriteRenderer iconRenderer)
        {
            var iconOffset = this.ItemIconOffset;
            var offsetY = spriteRenderer.DrawOrderOffsetY + iconOffset.Y;
            iconRenderer.PositionOffset = (x: spriteRenderer.PositionOffset.X + iconOffset.X,
                                           y: spriteRenderer.PositionOffset.Y + offsetY);
            iconRenderer.DrawOrderOffsetY -= offsetY;
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var worldObject = data.GameObject;
            var privateState = data.PrivateState;
            if (data.IsFirstTimeInit)
            {
                privateState.AccessMode = this.HasOwnersList
                                              ? WorldObjectAccessMode.OpensToObjectOwnersOrAreaOwners
                                              : WorldObjectAccessMode.OpensToEveryone;
            }
            else if (!this.HasOwnersList)
            {
                privateState.AccessMode = WorldObjectAccessMode.OpensToEveryone;
            }

            WorldObjectOwnersSystem.ServerInitialize(worldObject);

            var itemsContainer = privateState.ItemsContainer;
            if (itemsContainer is not null)
            {
                // container already created - update slots count
                Server.Items.SetSlotsCount(itemsContainer, slotsCount: this.ItemsSlotsCount);
                return;
            }

            itemsContainer = Server.Items.CreateContainer(
                owner: worldObject,
                itemsContainerType: this.ItemsContainerType,
                slotsCount: this.ItemsSlotsCount);

            privateState.ItemsContainer = itemsContainer;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((0.9, 0.475), offset: (0.05, 0.4))
                .AddShapeRectangle((0.9, 0.75),  offset: (0.05, 0.4), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((0.9, 0.85),  offset: (0.05, 0.4), group: CollisionGroups.ClickArea);
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced, timeInterval: 3, keyArgIndex: 0)]
        private void ServerRemote_SetIconSource(IStaticWorldObject worldObjectCrate, IProtoEntity iconSource)
        {
            this.VerifyGameObject(worldObjectCrate);

            var character = ServerRemoteContext.Character;
            if (!this.SharedCanInteract(character, worldObjectCrate, writeToLog: true))
            {
                return;
            }

            var publicState = GetPublicState(worldObjectCrate);
            publicState.IconSource = iconSource;
        }
    }

    public abstract class ProtoObjectCrate
        : ProtoObjectCrate<ObjectCratePrivateState, ObjectCratePublicState,
            StaticObjectClientState>
    {
    }
}