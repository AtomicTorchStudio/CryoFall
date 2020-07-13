namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Melee;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Cursor;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectLoot
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoStaticWorldObject
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectLoot,
          IProtoWorldObjectCustomInteractionCursor
        where TPrivateState : BasePrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public const string NotificationUseRightMouseButtonToPickup = "Use right mouse button to pick up.";

        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        public override string InteractionTooltipText => InteractionTooltipTexts.PickUp;

        public virtual bool IsAvailableInCompletionist => false;

        // we don't consider this as a floor object as we want decals appear under it
        public override StaticObjectKind Kind => StaticObjectKind.NaturalObject;

        public IReadOnlyDropItemsList LootDroplist { get; private set; }

        public override double ObstacleBlockDamageCoef => 0;

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        public override float StructurePointsMax => 1;

        protected virtual bool CanFlipSprite => true;

        public virtual CursorId GetInteractionCursorId(bool isCanInteract)
        {
            return isCanInteract
                       ? CursorId.PickupPossible
                       : CursorId.PickupImpossible;
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (0.5, 0.15);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            // flip renderer with some deterministic randomization
            if (this.CanFlipSprite
                && PositionalRandom.Get(data.GameObject.TilePosition, 0, 2, seed: 9125835) == 0)
            {
                data.ClientState.Renderer.DrawMode = DrawMode.FlipHorizontally;
            }
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            ClientSingleSimultaneousInteractionLimiter.InvokeForGameObject(
                data.GameObject,
                async () =>
                {
                    var result = await this.CallServer(_ => _.ServerRemote_Pickup(data.GameObject));
                    if (result == null)
                    {
                        this.SoundPresetObject.PlaySound(ObjectSound.InteractFail);
                        return;
                    }

                    if (this.SoundPresetObject.HasSound(ObjectSound.InteractSuccess))
                    {
                        this.SoundPresetObject.PlaySound(ObjectSound.InteractSuccess);
                    }
                    else
                    {
                        ItemsSoundPresets.ItemGeneric.PlaySound(ItemSound.Pick,
                                                                pitch: RandomHelper.Range(0.95f, 1.05f));
                    }

                    NotificationSystem.ClientShowItemsNotification(result);
                });
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
            return ObjectsSoundsPresets.ObjectGeneric
                                       .Clone()
                                       .Clear(ObjectSound.InteractSuccess);
        }

        protected virtual IReadOnlyDropItemsList ServerGetLootDroplist(IStaticWorldObject worldObject)
        {
            return this.LootDroplist;
        }

        protected override double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IStaticWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            if (weaponCache.ProtoWeapon is ItemNoWeapon)
            {
                // no damage with hands
                obstacleBlockDamageCoef = 1;

                if (IsClient)
                {
                    NotificationSystem.ClientShowNotification(NotificationUseRightMouseButtonToPickup,
                                                              icon: this.Icon);
                }

                return 0;
            }

            return base.SharedCalculateDamageByWeapon(weaponCache,
                                                      damagePreMultiplier,
                                                      targetObject,
                                                      out obstacleBlockDamageCoef);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(
                    radius: 0.45,
                    center: (0.5, 0.5),
                    group: CollisionGroups.ClickArea);
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        private void ClientRemote_OtherPlayerPickedUp(Vector2Ushort position)
        {
            var worldPosition = position.ToVector2D() + this.Layout.Center;
            if (this.SoundPresetObject.HasSound(ObjectSound.InteractSuccess))
            {
                this.SoundPresetObject.PlaySound(ObjectSound.InteractSuccess,
                                                 this,
                                                 worldPosition,
                                                 pitch: RandomHelper.Range(0.95f, 1.05f));
                return;
            }

            Client.Audio.PlayOneShot(ItemsSoundPresets.SoundResourceOtherPlayerPickItem,
                                     worldPosition,
                                     pitch: RandomHelper.Range(0.95f, 1.05f));
        }

        private CreateItemResult ServerRemote_Pickup(IStaticWorldObject worldObject)
        {
            var character = ServerRemoteContext.Character;
            // object proto validation
            if (worldObject.ProtoWorldObject != this)
            {
                throw new Exception("This is not a " + this);
            }

            // distance validation
            if (!this.SharedCanInteract(character, worldObject, writeToLog: true))
            {
                return null;
            }

            var lootDroplist = this.ServerGetLootDroplist(worldObject);
            var result = lootDroplist.TryDropToCharacterOrGround(character,
                                                                 worldObject.TilePosition,
                                                                 new DropItemContext(character, worldObject),
                                                                 groundContainer: out _);
            if (!result.IsEverythingCreated)
            {
                // cannot create all the drop items
                result.Rollback();
                return null;
            }

            using (var scopedBy = Api.Shared.GetTempList<ICharacter>())
            {
                Server.World.GetScopedByPlayers(worldObject, scopedBy);
                scopedBy.Remove(character);
                this.CallClient(scopedBy.AsList(),
                                _ => _.ClientRemote_OtherPlayerPickedUp(worldObject.TilePosition));
            }

            // destroy object after success pickup
            Server.World.DestroyObject(worldObject);

            // do not report those items that were dropped on the ground instead of player's containers
            result.RemoveEntriesNotOwnedByCharacter(character);
            return result;
        }
    }

    public abstract class ProtoObjectLoot
        : ProtoObjectLoot
            <EmptyPrivateState, StaticObjectPublicState, StaticObjectClientState>
    {
    }
}