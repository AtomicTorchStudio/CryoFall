namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    /// <summary>
    /// This is a special player character class which
    /// we're using to mimic the mob during the trailer recording.
    /// </summary>
    public class PlayerCharacterMob : PlayerCharacter
    {
        public override string Name => "Player character in Mob mode";

        public override double StatDefaultHealthMax => 200;

        public override double StatMoveSpeed => 1.5;

        public override double StatMoveSpeedRunMultiplier => 2;

        public override double StatRunningStaminaConsumptionUsePerSecond => 0;

        public static void ServerSwitchToMobMode(ICharacter character)
        {
            if (character.ProtoCharacter.GetType() == typeof(PlayerCharacterMob))
            {
                return;
            }

            // the order of calls is important here
            Server.Characters.SetSpectatorMode(character, isSpectator: false, reinitilize: false);
            Server.Characters.SetProto(character, GetProtoEntity<PlayerCharacterMob>());
        }

        public static void ServerSwitchToPlayerMode(ICharacter character)
        {
            if (character.ProtoCharacter.GetType() == typeof(PlayerCharacter))
            {
                return;
            }

            if (character.ProtoCharacter.GetType() != typeof(PlayerCharacterMob))
            {
                throw new Exception("Incorrect proto");
            }

            // the order of calls is important here
            Server.Characters.SetSpectatorMode(character, isSpectator: false, reinitilize: false);
            Server.Characters.SetProto(character, GetProtoEntity<PlayerCharacter>());
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            this.ClientSelectHotbarSlot(0);
        }

        protected override void SharedGetSkeletonProto(
            ICharacter character,
            out ProtoCharacterSkeleton protoSkeleton,
            ref double scale)
        {
            protoSkeleton = GetProtoEntity<SkeletonScorpion>();
        }
    }
}