namespace AtomicTorch.CBND.CoreMod.Systems.FactionLeaderboardSystem.Metrics
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDeath;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;

    /// <summary>
    /// This metric accumulates the Hunting skill experience gained by killing the creatures.
    /// </summary>
    public class FactionScoreMetricHuntingScore : ProtoFactionScoreMetric
    {
        public override double FinalScoreCoefficient => 0.5; // it's the XP->LP conversion coefficient for Hunting skill

        public override string Name => "Hunting score";

        public override double PowerCoefficient => 0.9;

        public override double ServerGetCurrentValue(ILogicObject faction)
        {
            return Faction.GetPrivateState(faction).ServerMetricHuntingScore;
        }

        public override void ServerInitialize()
        {
            ServerCharacterDeathMechanic.CharacterKilled += ServerCharacterDeathMechanicOnCharacterKilled;
        }

        private static void ServerCharacterDeathMechanicOnCharacterKilled(
            ICharacter attackerCharacter,
            ICharacter targetCharacter)
        {
            if (attackerCharacter.IsNpc
                || !targetCharacter.IsNpc)
            {
                return;
            }

            var faction = FactionSystem.ServerGetFaction(attackerCharacter);
            if (faction is null)
            {
                return;
            }

            // a mob is killed by player
            var rawXP = SkillHunting.ExperienceForKill;
            rawXP *= ((IProtoCharacterMob)targetCharacter.ProtoGameObject).MobKillExperienceMultiplier;
            if (rawXP <= 0)
            {
                return;
            }

            rawXP *= 0.01; // 0.01 is our default XP->LP conversion coefficient (see TechConstants)

            Faction.GetPrivateState(faction).ServerMetricHuntingScore += rawXP;
            /*Api.Logger.Dev("Mob killing metric updated: "
                           + rawXP.ToString("0.###")
                           + " total metric: "
                           + Faction.GetPrivateState(faction).ServerMetricHuntingScore.ToString("0.###"));*/
        }
    }
}