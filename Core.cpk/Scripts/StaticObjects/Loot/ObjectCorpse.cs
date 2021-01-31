namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectCorpse
        : ProtoStaticWorldObject
          <LootContainerPrivateState,
              ObjectCorpse.PublicState,
              StaticObjectClientState>,
          IProtoObjectGatherable
    {
        // Remove dead mob's corpse (and the mob game object itself) after this timeout.
        public const double CorpseTimeoutSeconds = 5 * 60;

        public override ITextureResource DefaultTexture => TextureResource.NoTexture;

        public double DurationGatheringSeconds => 4; // how long it takes to loot the body

        public override string InteractionTooltipText => InteractionTooltipTexts.Loot;

        // allow terrain decals under it
        public override StaticObjectKind Kind => StaticObjectKind.SpecialAllowDecals;

        public override string Name => "Corpse";

        // not played anyway
        public override ObjectMaterial ObjectMaterial => ObjectMaterial.SoftTissues;

        public override double ObstacleBlockDamageCoef => 0;

        public override double ServerUpdateIntervalSeconds => 10;

        public override float StructurePointsMax => 0; // non-damageable

        public static void ServerSetupCorpse(
            IStaticWorldObject objectCorpse,
            IProtoCharacterMob protoCharacterMob,
            Vector2F tileOffset,
            bool isFlippedHorizontally)
        {
            var publicState = GetPublicState(objectCorpse);
            publicState.ProtoCharacterMob = protoCharacterMob;
            publicState.TileOffset = tileOffset;
            publicState.IsFlippedHorizontally = isFlippedHorizontally;
            publicState.DeathTime = Server.Game.FrameTime;

            // re-initialize the object physics
            // (it's required because the physics should use
            // a proper tile offset and CorpseInteractionAreaScale from the mob prototype)
            objectCorpse.ProtoStaticWorldObject.SharedCreatePhysics(objectCorpse);
        }

        public double GetGatheringSpeedMultiplier(IStaticWorldObject worldObject, ICharacter character)
        {
            return character.SharedGetFinalStatMultiplier(StatName.HuntingLootingSpeed);
        }

        public bool ServerGather(IStaticWorldObject worldObject, ICharacter character)
        {
            var lootDroplist = GetPublicState(worldObject).ProtoCharacterMob
                                                          .LootDroplist;

            var dropItemContext = new DropItemContext(character, worldObject);
            CreateItemResult dropItemResult;
            var attemptRemains = 200;
            do
            {
                dropItemResult = lootDroplist.TryDropToCharacterOrGround(character,
                                                                         character.TilePosition,
                                                                         dropItemContext,
                                                                         out _);
            }
            // ensure that at least something is spawned...
            // perhaps that's not a good idea, but we have an attempts limit
            while (dropItemResult.TotalCreatedCount == 0
                   && --attemptRemains > 0);

            if (!dropItemResult.IsEverythingCreated)
            {
                Logger.Warning("Not all loot items were provided by "
                               + worldObject
                               + " - there is not enough space in inventory and around the character");
            }

            // probably the attempts limit exceeded and nothing is spawned
            // we don't consider this as an issue as the probability of this is too rare

            Logger.Info(worldObject + " was gathered", character);
            Server.World.DestroyObject(worldObject);

            NotificationSystem.ServerSendItemsNotification(character, dropItemResult);
            character.ServerAddSkillExperience<SkillHunting>(SkillHunting.ExperienceForGather);
            return true;
        }

        public override bool SharedCanInteract(
            ICharacter character,
            IStaticWorldObject staticWorldObject,
            bool writeToLog)
        {
            if (character.GetPublicState<ICharacterPublicState>().IsDead
                || IsServer && !character.ServerIsOnline)
            {
                return false;
            }

            return this.SharedIsInsideCharacterInteractionArea(character,
                                                               staticWorldObject,
                                                               writeToLog);
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            var publicState = GetPublicState((IStaticWorldObject)worldObject);
            var protoCharacterMob = publicState.ProtoCharacterMob;
            var result = protoCharacterMob.SharedGetObjectCenterWorldOffset(null);

            return result + publicState.TileOffset - (0, 0.3);
        }

        public bool SharedIsCanGather(IStaticWorldObject staticWorldObject)
        {
            return true;
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            obstacleBlockDamageCoef = 0;
            damageApplied = 0; // no damage
            return false;      // no hit
        }

        protected override ITextureResource ClientCreateIcon()
        {
            return this.DefaultTexture;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            //base.ClientInitialize(data);

            var publicState = data.PublicState;
            var worldOffset = publicState.TileOffset.ToVector2D();
            publicState.ProtoCharacterMob.SharedGetSkeletonProto(null,
                                                                 out var tempProtoSkeleton,
                                                                 out var scaleMultiplier);
            var protoSkeleton = (ProtoCharacterSkeleton)tempProtoSkeleton;

            var skeletonRenderer = Client.Rendering.CreateSkeletonRenderer(
                data.GameObject,
                new[] { protoSkeleton.SkeletonResourceFront },
                protoSkeleton.SkeletonResourceFront,
                protoSkeleton.DefaultAnimationName,
                positionOffset: worldOffset,
                worldScale: scaleMultiplier
                            * protoSkeleton.WorldScale);

            // play death animation
            skeletonRenderer.SetAnimation(AnimationTrackIndexes.Primary, "Death", isLooped: false);

            if (Client.CurrentGame.ServerFrameTimeRounded - publicState.DeathTime > 5)
            {
                // the corpse was spawned more than 5 seconds ago so let's skip the animation
                skeletonRenderer.SetAnimationTime(AnimationTrackIndexes.Primary, 10000);
            }

            skeletonRenderer.DrawMode = publicState.IsFlippedHorizontally
                                            ? DrawMode.FlipHorizontally
                                            : DrawMode.Default;

            protoSkeleton.OnSkeletonCreated(skeletonRenderer);
            var shadowRenderer = protoSkeleton.ClientCreateShadowRenderer(data.GameObject,
                                                                          scaleMultiplier);
            shadowRenderer.PositionOffset += worldOffset;
        }

        protected override void ClientInteractFinish(ClientObjectData data)
        {
            GatheringSystem.Instance.ClientTryAbortAction();
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            GatheringSystem.Instance.ClientTryStartAction();
        }

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            return ObjectsSoundsPresets.ObjectCorpse;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            var worldObject = data.GameObject;
            var publicState = data.PublicState;
            if (publicState.ProtoCharacterMob is null)
            {
                // incorrect game object
                Server.World.DestroyObject(worldObject);
                return;
            }

            var time = Server.Game.FrameTime;
            if (time < publicState.DeathTime + CorpseTimeoutSeconds)
            {
                return;
            }

            // should destroy because timed out
            if (Server.World.IsObservedByAnyPlayer(worldObject))
            {
                // there are players observing it
                return;
            }

            Server.World.DestroyObject(worldObject);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            var publicState = data.PublicState;
            if (publicState.ProtoCharacterMob is null)
            {
                return;
            }

            data.PhysicsBody
                .AddShapeCircle(radius: 0.6 * publicState.ProtoCharacterMob.CorpseInteractionAreaScale,
                                center: publicState.TileOffset.ToVector2D() + (0, 0.33),
                                group: CollisionGroups.ClickArea);
        }

        public class PublicState : StaticObjectPublicState
        {
            [SyncToClient(isSendChanges: false)]
            public double DeathTime { get; set; }

            [SyncToClient(isSendChanges: false)]
            public bool IsFlippedHorizontally { get; set; }

            [SyncToClient(isSendChanges: false)]
            public IProtoCharacterMob ProtoCharacterMob { get; set; }

            [SyncToClient(isSendChanges: false)]
            public Vector2F TileOffset { get; set; }
        }
    }
}