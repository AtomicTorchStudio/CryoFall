namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Melee;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesServer;

    public abstract class ProtoObjectGatherableVegetation
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectVegetation
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectGatherableVegetation
        where TPrivateState : VegetationPrivateState, new()
        where TPublicState : VegetationPublicState, new()
        where TClientState : VegetationClientState, new()
    {
        public const string NotificationPlantNotMatured
            = @"This plant hasn't matured yet.
  [br]You cannot harvest it yet.";

        // {0} contains according string from InteractionTooltipTexts
        public const string NotificationUseRightMouseButtonToInteract = "Use right mouse button to {0}.";

        public virtual double DurationGatheringSeconds => 2;

        public IReadOnlyDropItemsList GatherDroplist { get; private set; }

        public override string InteractionTooltipText => InteractionTooltipTexts.Gather;

        public virtual bool IsAutoDestroyOnGather => true;

        public override double ObstacleBlockDamageCoef => 0;

        public override float StructurePointsMax => 25;

        public override float CalculateShadowScale(VegetationClientState clientState)
        {
            return base.CalculateShadowScale(clientState) * 0.65f;
        }

        public virtual double GetGatheringSpeedMultiplier(IStaticWorldObject worldObject, ICharacter character)
        {
            return character.SharedGetFinalStatMultiplier(StatName.ForagingSpeed);
        }

        public bool ServerGather(IStaticWorldObject worldObject, ICharacter character)
        {
            if (!this.ServerTryGatherByCharacter(character, worldObject))
            {
                // cannot gather resource
                return false;
            }

            try
            {
                this.ServerOnGathered(worldObject, character);
            }
            finally
            {
                if (this.IsAutoDestroyOnGather)
                {
                    // destroy object after success gathering
                    Server.World.DestroyObject(worldObject);
                }
            }

            return true;
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            if (!base.SharedCanInteract(character, worldObject, writeToLog))
            {
                return false;
            }

            var publicState = GetPublicState(worldObject);
            // can interact only when the vegetation is full grown
            if (publicState.IsFullGrown(this))
            {
                return true;
            }

            if (writeToLog)
            {
                Logger.Warning(
                    $"Character cannot interact with {worldObject} - the plant is not matured yet.",
                    character);

                if (IsClient)
                {
                    ClientOnCannotInteract(
                        worldObject,
                        NotificationPlantNotMatured);
                }
            }

            return false;
        }

        public virtual bool SharedIsCanGather(IStaticWorldObject staticWorldObject)
        {
            var publicState = GetPublicState(staticWorldObject);
            return publicState.IsFullGrown(this);
        }

        protected override void ClientAddShadowRenderer(ClientInitializeData data)
        {
            base.ClientAddShadowRenderer(data);
            data.ClientState.RendererShadow.PositionOffset = (0.5, 0.28d);
        }

        protected override void ClientInteractFinish(ClientObjectData data)
        {
            GatheringSystem.Instance.ClientTryAbortAction();
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            GatheringSystem.Instance.ClientTryStartAction();
        }

        protected override void PrepareDroplistOnDestroy(DropItemsList droplist)
        {
        }

        protected abstract void PrepareGatheringDroplist(DropItemsList droplist);

        protected virtual void PrepareProtoGatherableVegetation()
        {
        }

        protected sealed override void PrepareProtoVegetation()
        {
            var gatherDroplist = new DropItemsList();
            this.PrepareGatheringDroplist(gatherDroplist);
            this.GatherDroplist = gatherDroplist.AsReadOnly();

            this.PrepareProtoGatherableVegetation();
        }

        protected virtual void ServerOnGathered(IStaticWorldObject worldObject, ICharacter byCharacter)
        {
            byCharacter.ServerAddSkillExperience<SkillForaging>(SkillForaging.ExperienceAddWhenGatheringFinished);
        }

        protected virtual bool ServerTryGatherByCharacter(ICharacter who, IStaticWorldObject vegetationObject)
        {
            var result = this.GatherDroplist.TryDropToCharacterOrGround(who,
                                                                        who.TilePosition,
                                                                        new DropItemContext(who, vegetationObject),
                                                                        out var groundItemsContainer);
            if (result.TotalCreatedCount > 0)
            {
                // even if at least one item is gathered it should pass
                // otherwise we will have an issue with berries and other stuff which cannot be rollback easily
                Logger.Info(vegetationObject + " was gathered", who);
                NotificationSystem.ServerSendItemsNotification(
                    who,
                    result,
                    exceptItemsContainer: groundItemsContainer);
                return true;
            }

            result.Rollback();
            return false;
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
                    NotificationSystem.ClientShowNotification(
                        string.Format(NotificationUseRightMouseButtonToInteract, this.InteractionTooltipText.ToLower()),
                        icon: this.Icon);
                }

                return 0;
            }

            return base.SharedCalculateDamageByWeapon(weaponCache,
                                                      damagePreMultiplier,
                                                      targetObject,
                                                      out obstacleBlockDamageCoef);
        }
    }

    public abstract class ProtoObjectGatherableVegetation
        : ProtoObjectGatherableVegetation
            <VegetationPrivateState,
                VegetationPublicState,
                VegetationClientState>
    {
    }
}