namespace AtomicTorch.CBND.CoreMod.RatesPresets
{
    using AtomicTorch.CBND.CoreMod.RatesPresets.Base;

    public class RatesPresetVanillaMultiplayer : BaseRatesPreset
    {
        public override string Description =>
            "Standard multiplayer settings with no rate changes applied. This is how the game is meant to be played in multiplayer.";

        public override bool IsMultiplayerOnly => true;

        public override string Name => "Regular multiplayer";

        public override BaseRatesPreset OrderAfterPreset
            => this.GetPreset<RatesPresetLocalServerHardcore>();

        protected override void PreparePreset(RatesPreset rates)
        {
            // vanilla server - all rates are default
        }
    }
}