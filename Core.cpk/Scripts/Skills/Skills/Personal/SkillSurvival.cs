namespace AtomicTorch.CBND.CoreMod.Skills
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterIdleSystem;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using static Stats.StatName;

    public class SkillSurvival : ProtoSkill
    {
        public const double ExperienceAddWhenOnlinePerSecond = 1.0;

        private const int TimerIntervalSeconds = 10;

        public override string Description =>
            "Increases your chances to stay alive by improving your maximum health, food, water and energy reserves. You are just that much tougher to kill!";

        // LP is not provided for this skill as it's too easy to gain just by being online and it's causing confusing LP gained notification
        public override double ExperienceToLearningPointsConversionMultiplier => 0;

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

                if (PlayerCharacter.GetPrivateState(character).IsDespawned
                    || PlayerCharacter.GetPublicState(character).IsDead)
                {
                    // despawned/dead characters are not processed
                    continue;
                }

                if (CharacterIdleSystem.ServerIsIdlePlayer(character))
                {
                    // idle character
                    continue;
                }

                character.ServerAddSkillExperience(this, experienceToAdd);
            }
        }
    }
}