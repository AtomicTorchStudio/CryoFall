namespace AtomicTorch.CBND.CoreMod.Skills
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using static Stats.StatName;

    public class SkillSurvival : ProtoSkill
    {
        public const double ExperienceAddWhenOnlinePerSecond = 1.0;

        private const int TimerIntervalSeconds = 10;

        public override string Description =>
            "Increases your chances to stay alive by improving your maximum health, food, water and energy reserves. You are just that much tougher to kill!";

        public override double ExperienceToLearningPointsConversionMultiplier =>
            0.2; // this is 1/5 of the standard conversion, because survival skill takes no effort from the player to advance

        public override bool IsSharingLearningPointsWithPartyMembers => false;

        public override string Name => "Survival";

        protected override void PrepareProtoSkill(SkillConfig config)
        {
            byte maxLevel = 20;

            config.Category = GetCategory<SkillCategoryPersonal>();
            config.MaxLevel = maxLevel;

            var stats = new[]
            {
                HealthMax,
                StaminaMax,
                FoodMax,
                WaterMax
            };

            foreach (var statName in stats)
            {
                config.AddStatEffect(
                    statName,
                    formulaPercentBonus: level => level);

                config.AddStatEffect(
                    statName,
                    level: maxLevel,
                    percentBonus: 5);
            }

            if (IsServer)
            {
                TriggerTimeInterval.ServerConfigureAndRegister(
                    TimeSpan.FromSeconds(TimerIntervalSeconds),
                    this.ServerTimerTick,
                    "Skill.SurvivalAddExperience");
            }
        }

        private void ServerTimerTick()
        {
            const double experienceToAdd = ExperienceAddWhenOnlinePerSecond
                                           * TimerIntervalSeconds;

            foreach (var character in Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true))
            {
                if (character.ProtoCharacter.GetType() != typeof(PlayerCharacter))
                {
                    // only characters of specific type (PlayerCharacter) are processed
                    continue;
                }

                var publicState = character.GetPublicState<ICharacterPublicState>();
                if (publicState.IsDead)
                {
                    // dead characters are not processed
                    continue;
                }

                character.ServerAddSkillExperience(this, experienceToAdd);
            }
        }
    }
}