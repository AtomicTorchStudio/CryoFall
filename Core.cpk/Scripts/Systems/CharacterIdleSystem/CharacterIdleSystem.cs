namespace AtomicTorch.CBND.CoreMod.Systems.CharacterIdleSystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Input;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class CharacterIdleSystem : ProtoSystem<CharacterIdleSystem>
    {
        private const double TimeIntervalSeconds = 1.0;

        public override string Name => "Character idle system";

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            // configure time interval trigger
            TriggerTimeInterval.ServerConfigureAndRegister(
                interval: TimeSpan.FromSeconds(TimeIntervalSeconds),
                callback: ServerTimerTickCallback,
                name: "System." + this.ShortId);
        }

        private static void ServerTimerTickCallback()
        {
            // update last active time for all online active players
            var serverTime = Server.Game.FrameTime;
            foreach (var character in Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true))
            {
                if (character.ProtoCharacter.GetType() != typeof(PlayerCharacter))
                {
                    // only characters of specific type (PlayerCharacter) are processed
                    continue;
                }

                var publicState = PlayerCharacter.GetPublicState(character);
                if (publicState.IsDead)
                {
                    // dead characters are not processed
                    continue;
                }

                var privateState = PlayerCharacter.GetPrivateState(character);
                var isIdlePlayer = publicState.AppliedInput.MoveModes == CharacterMoveModes.None
                                   && publicState.CurrentPublicActionState is null
                                   && !privateState.WeaponState.IsFiring;
                // please note that crafting is not considered as an activity to prevent AFK crafting players from thirst/hunger

                if (!isIdlePlayer)
                {
                    privateState.ServerLastActiveTime = serverTime;
                }
            }
        }
    }
}