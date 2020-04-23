namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.Technologies;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// This status effect is providing a daily bonus to players who cannot play the game a lot.
    /// </summary>
    public class StatusEffectWellRested : ProtoStatusEffect
    {
        /// <summary>
        /// The effect will deplete slowly with every gained LP.
        /// The number configured for this constant determines how long the full status effect will last.
        /// E.g. if we set it to 100 LP then 100% status effect will become 0% after gaining 100 LP
        /// (so 1 LP gained reduces this status effect by 1%).
        /// Currently we've set it to 200 LP
        /// (basically 100 LP gained by player and 100 LP gained via x2 bonus from this effect).
        /// </summary>
        public const int LearningPointsBonusForFullEffect = 200;

        private const double MinOfflineTimeToStartRestoringEffect = 2 * 60 * 60; // 2 hours

        private const double TimeIntervalSeconds = 60; // attempt to add effect once a minute

        private const double TimeToFullRestore = 20 * 60 * 60; // 20 hours

        private static IProtoStatusEffect instance;

        public override string Description =>
            "You've been resting for a while, so you find it easier to learn new things or polish your skills. (This is a daily bonus that accumulates while you are offline for a sufficiently long time; it is used to provide special bonuses as it depletes.)";

        public override bool IsPublic => false; // never show this effect to other players

        public override StatusEffectKind Kind => StatusEffectKind.Buff;

        public override string Name => "Well rested";

        public override double ServerUpdateIntervalSeconds => 60;

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddPercent(this, StatName.LearningsPointsGainMultiplier, 100)
                   .AddPercent(this, StatName.SkillsExperienceGainMultiplier, 100);
        }

        protected override void PrepareProtoStatusEffect()
        {
            instance = this;

            if (IsServer)
            {
                // setup timer (tick every frame)
                TriggerEveryFrame.ServerRegister(
                    callback: ServerGlobalUpdate,
                    name: "System." + this.ShortId);

                PlayerCharacterTechnologies.ServerCharacterGainedLearningPoints +=
                    this.ServerPlayerCharacterGainedLearningPointsHandler;
            }
        }

        private static void ServerGlobalUpdate()
        {
            var serverTime = Server.Game.FrameTime;

            // perform update once per configured interval per player
            const double spreadDeltaTime = TimeIntervalSeconds;

            // when player is subject to restore of the effect, add this amount
            const double intensityToAddPerInterval = spreadDeltaTime / TimeToFullRestore;

            using var tempListPlayers = Api.Shared.GetTempList<ICharacter>();
            PlayerCharacter.Instance
                           .EnumerateGameObjectsWithSpread(tempListPlayers.AsList(),
                                                           spreadDeltaTime: spreadDeltaTime,
                                                           Server.Game.FrameNumber,
                                                           Server.Game.FrameRate);

            foreach (var character in tempListPlayers.AsList())
            {
                var privateState = PlayerCharacter.GetPrivateState(character);
                var timeInOffline = serverTime - privateState.ServerLastActiveTime;
                if (timeInOffline < MinOfflineTimeToStartRestoringEffect)
                {
                    // player is offline for not enough time to receive the effect
                    continue;
                }

                // add a bit of effect
                character.ServerAddStatusEffect(instance, intensityToAddPerInterval);
            }
        }

        private void ServerPlayerCharacterGainedLearningPointsHandler(
            ICharacter character,
            int gainedLearningPoints,
            bool isModifiedByStat)
        {
            if (!isModifiedByStat)
            {
                // gained through the quest, by a consumable item, etc
                return;
            }

            var intensityToRemove = gainedLearningPoints
                                    / ((double)LearningPointsBonusForFullEffect
                                       * TechConstants.ServerLearningPointsGainMultiplier);
            character.ServerRemoveStatusEffectIntensity(this, intensityToRemove);

            //Logger.Dev(
            //    $"{character} gained: {gainedLearningPoints} LP. Intensity removed: {100 * intensityToRemove:F2}%");
        }
    }
}