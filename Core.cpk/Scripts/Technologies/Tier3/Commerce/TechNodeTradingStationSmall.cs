namespace AtomicTorch.CBND.CoreMod.Technologies.Tier3.Commerce
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;

    public class TechNodeTradingStationSmall : TechNode<TechGroupCommerceT3>
    {
        protected override void PrepareTechNode(Config config)
        {
            config.Effects
                  .AddStructure<ObjectTradingStationSmall>();

            config.SetRequiredNode<TechNodeCoinMinting>();
        }
    }
}