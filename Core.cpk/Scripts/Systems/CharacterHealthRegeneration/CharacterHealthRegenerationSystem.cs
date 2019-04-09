namespace AtomicTorch.CBND.CoreMod.Systems
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Triggers;

    public class CharacterHealthRegenerationSystem : ProtoSystem<CharacterHealthRegenerationSystem>
    {
        public override string Name => "Health regeneration system";

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                // only server will update health value
                return;
            }

            TriggerEveryFrame.ServerRegister(
                callback: Update,
                name: "System." + this.ShortId);
        }

        private static void Update()
        {
            // If update rate is 1, updating will happen for each character once a second.
            // We can set it to 2 to have updates every half second.
            const int updateRate = 1;
            var spread = Server.Game.FrameRate / updateRate;
            var frameNumberInSecond = Server.Game.FrameNumber % spread;

            // regenerate health for characters
            foreach (var character in Server.Characters.EnumerateAllCharacters())
            {
                if (character.Id % spread != frameNumberInSecond)
                {
                    // frame skip - this character will be not processed at this frame
                    continue;
                }

                var publicState = character.GetPublicState<ICharacterPublicState>();
                if (publicState.IsDead)
                {
                    // dead characters are not processed
                    continue;
                }

                var stats = publicState.CurrentStats;
                if (!character.IsNpc)
                {
                    if (stats.StaminaCurrent <= 0)
                    {
                        // cannot regenerate health - no energy
                        continue;
                    }
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