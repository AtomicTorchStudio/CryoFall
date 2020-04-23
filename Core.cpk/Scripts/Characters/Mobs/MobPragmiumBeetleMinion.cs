namespace AtomicTorch.CBND.CoreMod.Characters.Mobs
{
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class MobPragmiumBeetleMinion : MobPragmiumBeetle
    {
        public override bool AiIsRunAwayFromHeavyVehicles => false;

        public override bool IsAvailableInCompletionist => false;

        public override double StatMoveSpeed => 3.33;

        public override void ServerOnDeath(ICharacter character)
        {
            this.ServerSendDeathSoundEvent(character);

            // remove by timer
            ServerTimersSystem.AddAction(5,
                                         action: () => Server.World.DestroyObject(character));
        }

        protected override void PrepareProtoCharacterMob(
            out ProtoCharacterSkeleton skeleton,
            ref double scale,
            DropItemsList lootDroplist)
        {
            skeleton = GetProtoEntity<SkeletonPragmiumBeetle>();

            // no loot
            lootDroplist.Clear();
        }

        protected override void ServerUpdateMob(ServerUpdateData data)
        {
            if (data.PublicState.IsDead)
            {
                return;
            }

            var character = data.GameObject;

            ServerCharacterAiHelper.ProcessAggressiveAi(
                character,
                isRetreating: false,
                isRetreatingForHeavyVehicles: false,
                distanceRetreat: 0,
                distanceEnemyTooClose: 1,
                distanceEnemyTooFar: 16,
                movementDirection: out var movementDirection,
                rotationAngleRad: out var rotationAngleRad);

            this.ServerSetMobInput(character, movementDirection, rotationAngleRad);
        }
    }
}