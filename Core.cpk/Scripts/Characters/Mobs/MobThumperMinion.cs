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

        protected override double DistanceEnemyTooFar => 24;

        protected override double DistanceEnemyTooFarWhenAgro => 36;

        public override void ServerOnDeath(ICharacter character)
        {
            this.ServerSendDeathSoundEvent(character);

            // remove by timer
            ServerTimersSystem.AddAction(5,
                                         action: () => Server.World.DestroyObject(character));
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

            return base.SharedOnDamage(weaponCache,
                                       targetObject,
                                       damagePreMultiplier,
                                       damagePostMultiplier,
                                       out obstacleBlockDamageCoef,
                                       out damageApplied);
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