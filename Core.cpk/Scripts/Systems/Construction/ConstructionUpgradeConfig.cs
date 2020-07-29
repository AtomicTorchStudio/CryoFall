namespace AtomicTorch.CBND.CoreMod.Systems.Construction
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConstructionUpgradeConfig : IConstructionUpgradeConfigReadOnly
    {
        private List<ConstructionUpgradeEntry> entries;

        public IReadOnlyList<IConstructionUpgradeEntryReadOnly> Entries
        {
            get
            {
                if (this.entries != null)
                {
                    return this.entries;
                }

                return Array.Empty<IConstructionUpgradeEntryReadOnly>();
            }
        }

        public ConstructionUpgradeEntry AddUpgrade<TProtoObjectStructure>()
            where TProtoObjectStructure : IProtoObjectStructure, new()
        {
            this.entries ??= new List<ConstructionUpgradeEntry>();

            var protoStructure = (IProtoObjectStructure)Api.GetProtoEntity<TProtoObjectStructure>();
            foreach (var existingEntry in this.entries)
            {
                if (existingEntry.ProtoStructure == protoStructure)
                {
                    throw new Exception("Upgrade is already added: " + protoStructure);
                }
            }

            var entry = new ConstructionUpgradeEntry(protoStructure);

            this.entries.Add(entry);
            return entry;
        }

        public void ApplyRates(byte multiplier)
        {
            if (this.entries == null)
            {
                return;
            }

            foreach (var entry in this.entries)
            {
                entry.ApplyRates(multiplier);
            }
        }
    }
}