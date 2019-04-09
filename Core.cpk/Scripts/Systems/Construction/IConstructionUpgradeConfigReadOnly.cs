namespace AtomicTorch.CBND.CoreMod.Systems.Construction
{
    using System.Collections.Generic;

    public interface IConstructionUpgradeConfigReadOnly
    {
        IReadOnlyList<IConstructionUpgradeEntryReadOnly> Entries { get; }
    }
}