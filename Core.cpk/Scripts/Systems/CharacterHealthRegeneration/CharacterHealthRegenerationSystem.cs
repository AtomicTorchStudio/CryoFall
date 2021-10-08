namespace AtomicTorch.CBND.CoreMod.Systems
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi;

    public class CharacterHealthRegenerationSystem : ProtoSystem<CharacterHealthRegenerationSystem>
    {
        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                // only server will update health value
                return;
            }

            TriggerEveryFrame.ServerRegister(
                callback: ServerUpdate,
                name: "System." + this.ShortId);
        }

        private static void ServerUpdate()
        {
            // If update rate is 1, updating will happen for each character once a second.
            // We can set it to 2 to have updates every half second.
            // If we set it to 1/10.0 it will update every 10 seconds and so on.
            const double updateRate = 1 / 10.0;

            foreach (var character in Server.Characters.EnumerateAllCharactersWithSpread(updateRate,
                exceptSpectators: false))
            {
                var publicState = character.GetPublicState<ICharacterPublicState>();
                if (publicState.IsDead
                    || !character.IsNpc && PlayerCharacter.GetPrivateState(character).IsDespawned)
                {
                    // dead/despawned characters are not processed
                    continue;
                }

                if (!character.ServerIsOnline)
                {
                    // don't regenerate health for offline players
                    continue;
                }

                var stats = publicState.CurrentStats;
                if (!character.IsNpc
                    && ((PlayerCharacterCurrentStats)stats).StaminaCurrent <= 0)
                {
                    // cannot regenerate health - no energy
                    continue;
                }

                var healthRegeneration = character.SharedGetFinalStatValue(StatName.HealthRegenerationPerSecond);
                if (healthRegeneration <= 0)
                {
                    // don't regenerate health for this character
                    continue;
                }

                var newHealth = stats.HealthCurrent + (float)(healthRegeneration / updateRate);
                stats.ServerSetHealthCurrent(newHealth);
            }
        }
    }
}