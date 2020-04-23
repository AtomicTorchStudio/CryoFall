namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.Cursor;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Hacking;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Hacking;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectHackableContainer
        : ProtoStaticWorldObject
          <EmptyPrivateState,
              LootHackingContainerPublicState,
              StaticObjectClientState>,
          IProtoObjectHackableContainer
    {
        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        public abstract double HackingStageDuration { get; }

        public abstract double HackingStagesNumber { get; }

        public override string InteractionTooltipText => InteractionTooltipTexts.Hack;

        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        public IReadOnlyDropItemsList LootDroplist { get; private set; }

        public override double ObstacleBlockDamageCoef => 0;

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        public override float StructurePointsMax => 0; // it's non-damageable

        protected virtual bool CanFlipSprite => true;

        public virtual CursorId GetInteractionCursorId(bool isCanInteract)
        {
            return isCanInteract
                       ? CursorId.PickupPossible
                       : CursorId.PickupImpossible;
        }

        public bool ServerOnHackingStage(IStaticWorldObject worldObject, ICharacter character)
        {
            var publicState = GetPublicState(worldObject);
            publicState.HackingProgressPercent += 100 / this.HackingStagesNumber;
            if (publicState.HackingProgressPercent < 100)
            {
                return true;
            }

            Server.World.DestroyObject(worldObject);

            var lootDroplist = this.LootDroplist;
            var dropItemContext = new DropItemContext(character, worldObject);

            var dropItemResult = lootDroplist.TryDropToCharacterOrGround(character,
                                                                         character.TilePosition,
                                                                         dropItemContext,
                                                                         out _);

            if (dropItemResult.TotalCreatedCount > 0)
            {
                NotificationSystem.ServerSendItemsNotification(character, dropItemResult);
                //character.ServerAddSkillExperience<SkillSearching>(skillExperienceToAdd);
            }

            return true;
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            if (!base.SharedCanInteract(character, worldObject, writeToLog))
            {
                return false;
            }

            if (NewbieProtectionSystem.SharedIsNewbie(character))
            {
                // don't allow hacking while under newbie protection
                if (writeToLog && IsClient)
                {
                    NewbieProtectionSystem.ClientNotifyNewbieCannotPerformAction(this);
                }

                return false;
            }

            return true;
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (this.Layout.Center.X, 0.5);
        }

        public sealed override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
            damageApplied = 0; // no damage
            return true;       // hit
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            // hacking progress display
            Api.Client.UI.AttachControl(
                data.GameObject,
                new ObjectHackingProgressDisplayControl(data.PublicState),
                positionOffset: this.SharedGetObjectCenterWorldOffset(data.GameObject) + (0, 1.3),
                isFocusable: false);

            // flip renderer with some deterministic randomization
            if (this.CanFlipSprite
                && PositionalRandom.Get(data.GameObject.TilePosition, 0, 2, seed: 9125835) == 0)
            {
                data.ClientState.Renderer.DrawMode = DrawMode.FlipHorizontally;
            }
        }

        protected override void ClientInteractFinish(ClientObjectData data)
        {
            HackingSystem.ClientTryAbortAction();
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            HackingSystem.ClientTryStartAction();
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
            return ObjectsSoundsPresets.ObjectHackableContainer;
        }
    }
}