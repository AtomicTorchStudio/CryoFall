namespace AtomicTorch.CBND.CoreMod.Systems.CharacterEnergySystem
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class CharacterEnergyRegenerationSystem : ProtoSystem<CharacterEnergyRegenerationSystem>
    {
        private const double TimeIntervalSeconds = 10.0;

        public override string Name => "Character energy regeneration system";

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                // only server will regenerate the energy
                return;
            }

            // configure time interval trigger
            TriggerTimeInterval.ServerConfigureAndRegister(
                interval: TimeSpan.FromSeconds(TimeIntervalSeconds),
                callback: this.ServerTimerTickCallback,
                name: "System." + this.ShortId);
        }

        private void ServerTimerTickCallback()
        {
            // update for all online player characters
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

                var energyAmountToAdd = TimeIntervalSeconds
                                        * character.SharedGetFinalStatValue(StatName.EnergyChargeRegenerationPerMinute)
                                        / 60.0;

                if (energyAmountToAdd > 0)
                {
                    CharacterEnergySystem.ServerAddEnergyCharge(character, energyAmountToAdd);
                }
            }
        }
    }
}