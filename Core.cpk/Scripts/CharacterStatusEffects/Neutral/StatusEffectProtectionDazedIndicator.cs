namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Neutral
{
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    /// <summary>
    /// This is a special protection class that is displayed over character that has a dazed protection
    /// that is added via StatName.DazedIncreaseRateMultiplier by Drunk status effect and Endoskeletal reinforcement implant.
    /// It doesn't provide any effect on its own and used only to indicate the presence of dazed protection
    /// to make it obvious for players around.
    /// </summary>
    public class StatusEffectProtectionDazedIndicator : ProtoStatusEffect
    {
        [NotLocalizable]
        public override string Description => string.Empty;

        public override StatusEffectDisplayMode DisplayMode => StatusEffectDisplayMode.None;

        // does not decrease
        public override double IntensityAutoDecreasePerSecondValue => 0;

        // visible to other players as an icon over player
        public override bool IsPublic => true;

        public override bool IsRemovedOnRespawn => false;

        public override StatusEffectKind Kind => StatusEffectKind.Neutral;

        [NotLocalizable]
        public override string Name => "Dazed protection";

        public override double ServerAutoAddRepeatIntervalSeconds => 2;

        // invisible to the player
        public override double VisibilityIntensityThreshold => double.MaxValue;

        protected override void ServerOnAutoAdd(ICharacter character)
        {
            if (ServerHasDazedProtection(character))
            {
                character.ServerAddStatusEffect(this, 1);
            }
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            var hasProtection = ServerHasDazedProtection(data.Character);
            data.Intensity = hasProtection ? 1 : 0;
        }

        private static bool ServerHasDazedProtection(ICharacter character)
        {
            return character.SharedGetFinalStatMultiplier(StatName.DazedIncreaseRateMultiplier)
                   <= 0;
        }
    }
}