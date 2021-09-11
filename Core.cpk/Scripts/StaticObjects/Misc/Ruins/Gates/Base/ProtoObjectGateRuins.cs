namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Ruins.Gates
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;

    public abstract class ProtoObjectGateRuins
        : ProtoObjectGate<
            ObjectGateRuinsPrivateState,
            ObjectDoorPublicState,
            ObjectDoorClientState>
    {
        public override string Description => null;

        public override string InteractionTooltipText => null;

        public override bool IsInteractableObject => false;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 0; // non-damageable

        public override void ServerApplyDecay(IStaticWorldObject worldObject, double deltaTime)
        {
            // cannot decay
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            return false;
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
            // don't use base implementation
            //base.ClientInitialize(data);

            var publicState = data.PublicState;
            var clientState = data.ClientState;

            this.ClientSetupDoor(data);

            // subscribe on IsOpened change
            var staticWorldObject = data.GameObject;
            publicState.ClientSubscribe(
                _ => _.IsOpened,
                newIsOpened =>
                {
                    this.SharedCreatePhysics(staticWorldObject);
                    clientState.SpriteAnimator.Start(isPositiveDirection: newIsOpened);
                    Client.Audio.PlayOneShot(
                        this.SoundResourceDoorStart,
                        staticWorldObject,
                        volume: this.SoundsVolume);

                    Client.Audio.PlayOneShot(
                        this.SoundResourceDoorEnd,
                        staticWorldObject,
                        delay: this.DoorOpenCloseAnimationDuration - this.DoorOpenCloseAnimationDuration / 5f,
                        volume: this.SoundsVolume);
                },
                subscriptionOwner: clientState);

            // subscribe on IsHorizontalDoor change
            publicState.ClientSubscribe(
                _ => _.IsHorizontalDoor,
                newIsHorizontal => this.ClientSetupDoor(data),
                subscriptionOwner: clientState);
        }

        protected override void PrepareConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            // it's not buildable
            category = GetCategory<StructureCategoryOther>();
            build.IsAllowed = false;
            repair.IsAllowed = false;
        }

        protected override double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IStaticWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
            return 0;
        }
    }
}