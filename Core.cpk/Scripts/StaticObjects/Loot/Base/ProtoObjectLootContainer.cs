namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.InteractionChecker;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Windows;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectLootContainer
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoStaticWorldObject
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectLoot,
          IProtoObjectGatherable
        where TPrivateState : LootContainerPrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public virtual double DurationGatheringSeconds => 4;

        public override string InteractionTooltipText => InteractionTooltipTexts.PryOpen;

        public virtual bool IsAutoDestroyWhenLooted { get; } = true;

        public byte ItemsSlotsCount => this.MaxItemsSlotsCount;

        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        public IReadOnlyDropItemsList LootDroplist { get; private set; }

        public virtual byte MaxItemsSlotsCount => 16;

        protected virtual IProtoItemsContainer ItemsContainerType
            => Api.GetProtoEntity<ItemsContainerOutput>();

        public override string ClientGetTitle(IStaticWorldObject worldObject)
        {
            // this is not a player-built structure so let's hide the name
            return null;
        }

        public double GetGatheringSpeedMultiplier(IStaticWorldObject worldObject, ICharacter character)
        {
            return character.SharedGetFinalStatMultiplier(StatName.SearchingSpeed);
        }

        public bool ServerGather(IStaticWorldObject worldObject, ICharacter character)
        {
            var privateState = GetPrivateState(worldObject);
            if (privateState.IsDropListSpawned)
            {
                // this loot container was already search - drop list was already spawned
                return true;
            }

            // spawn items accordingly to the droplist
            privateState.IsDropListSpawned = true;

            var lootDroplist = this.ServerGetLootDroplist(worldObject);
            var dropItemContext = new DropItemContext(character, worldObject);
            CreateItemResult dropItemResult;
            var attemptRemains = 100;
            var itemsContainer = privateState.ItemsContainer;
            do
            {
                dropItemResult = lootDroplist.TryDropToContainer(itemsContainer, dropItemContext);
            }
            // ensure that at least something is spawned...
            // perhaps that's not a good idea, but we have an attempts limit
            while (dropItemResult.TotalCreatedCount == 0
                   && --attemptRemains > 0);

            Server.Items.SetSlotsCount(itemsContainer, (byte)itemsContainer.OccupiedSlotsCount);

            character.ServerAddSkillExperience<SkillSearching>(SkillSearching.ExperienceAddWhenSearching);

            Server.World.EnterPrivateScope(character, worldObject);

            // register private scope exit on interaction cancel
            InteractionCheckerSystem.SharedRegister(
                character,
                worldObject,
                finishAction: isAbort =>
                              {
                                  if (worldObject.IsDestroyed)
                                  {
                                      return;
                                  }

                                  Server.World.ExitPrivateScope(character, worldObject);

                                  if (isAbort)
                                  {
                                      // notify client
                                      this.CallClient(character, _ => _.ClientRemote_FinishInteraction(worldObject));
                                  }

                                  if (this.IsAutoDestroyWhenLooted)
                                  {
                                      // container was closed - destroy it
                                      Server.World.DestroyObject(worldObject);
                                  }
                              });

            Logger.Important($"Started object interaction with {worldObject} for {character}");
            this.CallClient(character, _ => _.ClientRemote_OnContainerOpened(worldObject));
            return true;
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);

            var itemsContainer = GetPrivateState(gameObject).ItemsContainer;
            if (itemsContainer.OccupiedSlotsCount > 0)
            {
                ObjectGroundItemsContainer.ServerTryDropOnGroundContainerContent(
                    gameObject.OccupiedTile,
                    itemsContainer);
            }
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
            => base.SharedGetObjectCenterWorldOffset(worldObject)
               + (0, 0.15);

        public bool SharedIsCanGather(IStaticWorldObject staticWorldObject)
        {
            return true;
        }

        protected override void ClientInteractFinish(ClientObjectData data)
        {
            GatheringSystem.Instance.ClientTryAbortAction();
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            GatheringSystem.Instance.ClientTryStartAction();
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureResource($"StaticObjects/Loot/{this.GetType().Name}.png");
        }

        protected abstract void PrepareLootDroplist(DropItemsList droplist);

        protected override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            var droplist = new DropItemsList();
            this.PrepareLootDroplist(droplist);
            this.LootDroplist = droplist.AsReadOnly();
        }

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            return ObjectsSoundsPresets.ObjectLockedContainer;
        }

        protected virtual IReadOnlyDropItemsList ServerGetLootDroplist(IStaticWorldObject crateObject)
            => this.LootDroplist;

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var itemsContainer = data.PrivateState.ItemsContainer;
            if (itemsContainer != null)
            {
                // container already created - update slots count
                Server.Items.SetSlotsCount(itemsContainer, slotsCount: this.ItemsSlotsCount);
                return;
            }

            itemsContainer = Server.Items.CreateContainer(
                owner: data.GameObject,
                itemsContainerType: this.ItemsContainerType,
                slotsCount: this.ItemsSlotsCount);

            data.PrivateState.ItemsContainer = itemsContainer;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            if (!this.IsAutoDestroyWhenLooted)
            {
                return;
            }

            var privateState = data.PrivateState;
            if (!privateState.IsDropListSpawned)
            {
                // no droplist spawned
                return;
            }

            if (privateState.ItemsContainer.OccupiedSlotsCount > 0)
            {
                // container will be automatically destroyed when client interaction finishes
                return;
            }

            // container looted completely - destroy it
            Server.World.DestroyObject(data.GameObject);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle((1, 0.5))
                .AddShapeRectangle((1, 1), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((1, 1), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle((1, 1), group: CollisionGroups.ClickArea);
        }

        private void ClientRemote_FinishInteraction(IStaticWorldObject worldObject)
        {
            Logger.Important($"Server informed that the object interaction with {worldObject} is finished");
            ClientInteractionUISystem.OnServerForceFinishInteraction(worldObject);
        }

        private void ClientRemote_OnContainerOpened(IStaticWorldObject worldObject)
        {
            var privateState = GetPrivateState(worldObject);
            var itemsContainer = privateState.ItemsContainer;

            var soundClose = Client.UI.GetApplicationResource<SoundUI>("SoundWindowContainerClose");
            var menuWindow = WindowContainerExchange.Show(
                itemsContainer,
                soundClose: soundClose,
                isAutoClose: true);

            var character = Client.Characters.CurrentPlayerCharacter;
            InteractionCheckerSystem.SharedRegister(
                character,
                worldObject,
                finishAction: _ => menuWindow.CloseWindow());

            ClientInteractionUISystem.Register(
                worldObject,
                menuWindow,
                onMenuClosedByClient:
                () =>
                {
                    InteractionCheckerSystem.SharedUnregister(character, worldObject, isAbort: false);
                    if (!worldObject.IsDestroyed)
                    {
                        this.CallServer(_ => _.ServerRemote_OnClientInteractFinish(worldObject));
                    }
                });

            Logger.Important("Started object interaction with " + worldObject);

            ClientCurrentInteractionMenu.RegisterMenuWindow(menuWindow);
            ClientCurrentInteractionMenu.Open();
        }

        private void ServerRemote_OnClientInteractFinish(IStaticWorldObject worldObject)
        {
            var character = ServerRemoteContext.Character;
            if (!InteractionCheckerSystem.SharedUnregister(character, worldObject, isAbort: false))
            {
                return;
            }

            Logger.Important($"Client {character} informed that the object interaction with {worldObject} is finished");
        }
    }

    public abstract class ProtoObjectLootContainer
        : ProtoObjectLootContainer
            <LootContainerPrivateState, StaticObjectPublicState, StaticObjectClientState>
    {
    }
}