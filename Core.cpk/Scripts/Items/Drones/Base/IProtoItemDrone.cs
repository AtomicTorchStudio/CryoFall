namespace AtomicTorch.CBND.CoreMod.Items.Drones
{
    using AtomicTorch.CBND.CoreMod.Drones;

    public interface IProtoItemDrone : IProtoItemWithDurability
    {
        double DurabilityToStructurePointsConversionCoefficient { get; }

        IProtoDrone ProtoDrone { get; }

        /// <summary>
        /// This number is used during drone selection when player sending a drone to mine something.
        /// </summary>
        int SelectionOrder { get; }
    }
}