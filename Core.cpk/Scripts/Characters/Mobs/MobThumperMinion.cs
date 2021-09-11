namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class MobThumperMinion : MobThumper
    {
        public override bool IsAvailableInCompletionist => false;

        public override double StatDefaultHealthMax => 250;

        protected override double DistanceEnemyTooFar => 30;

        protected override double DistanceEnemyTooFarWhenAggro => 40;

        public override void ServerOnDeath(ICharacter character)
        {
            this.ServerSendDeathSoundEvent(character);

            ServerTimersSystem.AddAction(
                delaySeconds: 5,
                () => this.ServerSetSpawnState(character, MobSpawnState.Despawning));
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IWorldObject targetObject,
            double damagePreMultiplier,
            double damagePostMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            if (weaponCache.ProtoExplosive is ObjectMobSandTyrantMissileExplosion)
            {
                // higher damage from the boss missiles
                damagePreMultiplier *= 1.5;
            }

            var byCharacter = weaponCache.Character;
            var result = base.SharedOnDamage(weaponCache,
                                             targetObject,
                                             damagePreMultiplier,
                                             damagePostMultiplier,
                                             out obstacleBlockDamageCoef,
                                             out damageApplied);

            if (IsServer
                && result
                && byCharacter is not null
                && !byCharacter.IsNpc)
            {
                // record the damage dealt by player
                var targetCharacter = (ICharacter)targetObject;
                var privateState = GetPrivateState(targetCharacter);
                privateState.DamageTracker?.RegisterDamage(byCharacter, targetCharacter, damageApplied);
            }

            return result;
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            base.PrepareProtoCharacterMob(out skeleton, ref scale, lootDroplist);
            lootDroplist.Clear();
        }
    }
}