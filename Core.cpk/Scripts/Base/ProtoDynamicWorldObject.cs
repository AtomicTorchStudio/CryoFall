namespace AtomicTorch.CBND.CoreMod
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Primitives;
    using JetBrains.Annotations;

    public abstract class ProtoDynamicWorldObject
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoWorldObject
          <IDynamicWorldObject,
              TPrivateState,
              TPublicState,
              TClientState>,
          IProtoDynamicWorldObject,
          IDamageableProtoWorldObject
        where TPrivateState : BasePrivateState, new()
        where TPublicState : BasePublicState, IPublicStateWithStructurePoints, new()
        where TClientState : BaseClientState, new()
    {
        public FinalStatsCache DefenseStats { get; private set; }

        public abstract float ObjectSoundRadius { get; }

        public virtual double ObstacleBlockDamageCoef => 1;

        public abstract float StructurePointsMax { get; }

        public void ServerApplyDamage(IDynamicWorldObject worldObject, double damage)
        {
            this.VerifyGameObject(worldObject);

            var publicState = GetPublicState(worldObject);
            var newStructurePoints = (float)(publicState.StructurePointsCurrent - damage);
            bool isDestroyed;

            if (newStructurePoints > 0)
            {
                isDestroyed = false;
            }
            else
            {
                newStructurePoints = 0;
                isDestroyed = true;
            }

            publicState.StructurePointsCurrent = newStructurePoints;

            if (isDestroyed)
            {
                this.ServerOnDynamicObjectZeroStructurePoints(null, null, worldObject);
            }
        }

        public virtual float SharedGetStructurePointsMax(IDynamicWorldObject worldObject)
        {
            return this.StructurePointsMax;
        }

        public bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IWorldObject targetObject,
            double damagePreMultiplier,
            double damagePostMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            var targetWorldDynamic = (IDynamicWorldObject)targetObject;
            var objectPublicState = GetPublicState(targetWorldDynamic);
            var previousStructurePoints = objectPublicState.StructurePointsCurrent;
            if (previousStructurePoints <= 0f)
            {
                // already destroyed static world object
                obstacleBlockDamageCoef = 0;
                damageApplied = 0;
                return false;
            }

            var serverDamage = this.SharedCalculateDamageByWeapon(
                weaponCache,
                damagePreMultiplier,
                targetWorldDynamic,
                out obstacleBlockDamageCoef);

            if (serverDamage < 0)
            {
                Logger.Warning(
                    $"Server damage less than 0 and this is suspicious. {this} calculated damage: {serverDamage:0.###}");
                serverDamage = 0;
            }

            if (IsClient)
            {
                damageApplied = 0;
                return true;
            }

            // apply damage
            damageApplied = serverDamage;
            var newStructurePoints = (float)(previousStructurePoints - serverDamage);
            if (newStructurePoints < 0)
            {
                newStructurePoints = 0;
            }

            Logger.Info(
                $"Damage applied to {targetObject} by {weaponCache.Character}:\n{serverDamage} dmg, current structure points {newStructurePoints}/{this.StructurePointsMax}, {weaponCache.Weapon}");

            objectPublicState.StructurePointsCurrent = newStructurePoints;

            try
            {
                this.ServerOnDynamicObjectDamageApplied(
                    weaponCache,
                    targetWorldDynamic,
                    previousStructurePoints,
                    newStructurePoints);
            }
            catch (Exception ex)
            {
                Logger.Exception(ex, $"Problem on processing {nameof(this.ServerOnDynamicObjectDamageApplied)}()");
            }

            if (newStructurePoints <= 0f)
            {
                this.ServerOnDynamicObjectZeroStructurePoints(weaponCache, weaponCache.Character, targetObject);
            }

            return true;
        }

        protected virtual void ClientAddAutoStructurePointsBar(ClientInitializeData data)
        {
            var worldObject = data.GameObject;
            var sceneObject = worldObject.ClientSceneObject;
            sceneObject.AddComponent<ClientComponentAutoDisplayStructurePointsBar>()
                       .Setup(worldObject,
                              structurePointsMax: this.StructurePointsMax,
                              // always display the structure points bar
                              customDamageThresholdFraction: 1.0001);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            this.ClientAddAutoStructurePointsBar(data);
        }

        protected override void ClientOnObjectDestroyed(Vector2D position)
        {
        }

        protected virtual void PrepareDefense(DefenseDescription defense)
        {
        }

        protected virtual void PrepareProtoDynamicWorldObject()
        {
        }

        protected sealed override void PrepareProtoWorldObject()
        {
            base.PrepareProtoWorldObject();

            var defenseDescription = new DefenseDescription();

            // Set default defense for static objects. By default there is no defense except for psi and radiation which are set to Max.
            // Individual objects will override this value with some specific tier of defense in their own class.
            defenseDescription.Set(ObjectDefensePresets.Default);

            this.PrepareDefense(defenseDescription);
            var defense = defenseDescription.ToReadOnly();

            using var effects = TempStatsCache.GetFromPool();
            defense.FillEffects(this, effects, maximumDefensePercent: double.MaxValue);
            this.DefenseStats = effects.CalculateFinalStatsCache();

            this.PrepareProtoDynamicWorldObject();
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var worldObject = data.GameObject;
            var publicState = data.PublicState;

            var structurePointsMax = this.SharedGetStructurePointsMax(worldObject);
            if (data.IsFirstTimeInit)
            {
                publicState.StructurePointsCurrent = structurePointsMax;
                return;
            }

            if (publicState.StructurePointsCurrent <= 0)
            {
                // this is possible in case the savegame was made when object was awaiting destruction
                Server.World.DestroyObject(data.GameObject);
                return;
            }

            publicState.StructurePointsCurrent = Math.Min(publicState.StructurePointsCurrent, structurePointsMax);
        }

        protected virtual void ServerOnDynamicObjectDamageApplied(
            WeaponFinalCache weaponCache,
            IDynamicWorldObject targetObject,
            float previousStructurePoints,
            float currentStructurePoints)
        {
        }

        protected virtual void ServerOnDynamicObjectDestroyedByCharacter(
            [CanBeNull] ICharacter byCharacter,
            [CanBeNull] IProtoItemWeapon byWeaponProto,
            IDynamicWorldObject targetObject)
        {
        }

        protected virtual void ServerOnDynamicObjectZeroStructurePoints(
            [CanBeNull] WeaponFinalCache weaponCache,
            [CanBeNull] ICharacter byCharacter,
            [NotNull] IWorldObject targetObject)
        {
            if (targetObject.IsDestroyed)
            {
                return;
            }

            Logger.Info($"Dynamic object destroyed: {targetObject} by {byCharacter}");

            this.ServerSendObjectDestroyedEvent(targetObject);
            Server.World.DestroyObject(targetObject);

            if (weaponCache == null)
            {
                return;
            }

            var worldObject = (IDynamicWorldObject)targetObject;
            ServerDynamicObjectDestroyObserver.NotifyObjectDestroyed(
                byCharacter,
                worldObject);

            this.ServerOnDynamicObjectDestroyedByCharacter(
                byCharacter,
                weaponCache.ProtoWeapon,
                worldObject);
        }

        protected virtual double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IDynamicWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
            if (IsClient)
            {
                // we don't apply any damage on the Client-side
                return 0;
            }

            return WeaponDamageSystem.ServerCalculateTotalDamage(
                weaponCache,
                targetObject,
                this.DefenseStats,
                damagePreMultiplier,
                clampDefenseTo1: false);
        }
    }
}